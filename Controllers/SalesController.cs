using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        // Hiển thị màn hình bán hàng
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách món đang bán
            var foods = await _context.Foods
                .Where(f => f.IsAvailable)
                .OrderBy(f => f.CategoryId)
                .ThenBy(f => f.Id)
                .ToListAsync();

            // Lấy danh sách bàn
            var tables = await _context.RestaurantTables
                .OrderBy(t => t.Id)
                .ToListAsync();

            ViewBag.Tables = tables;

            return View(foods);
        }


        // =====================================================
        // LẤY HOẶC TẠO HÓA ĐƠN ĐANG MỞ CỦA MỘT BÀN
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetTableOrder(int tableId)
{
    // Kiểm tra bàn có tồn tại không
    var table = await _context.RestaurantTables
        .FirstOrDefaultAsync(t => t.Id == tableId);

    if (table == null)
    {
        return NotFound();
    }


    // =========================================
    // TÌM HÓA ĐƠN ĐANG MỞ CỦA BÀN
    // =========================================

    var order = await _context.Orders
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Food)
        .FirstOrDefaultAsync(o =>
            o.TableId == tableId &&
            o.OrderType == "DineIn" &&
            o.Status == "Open"
        );


    // =========================================
    // XỬ LÝ ĐƠN RỖNG CŨ
    // =========================================
    // Trước đây bấm vào bàn là hệ thống tạo Order ngay.
    // Nếu còn Order cũ nhưng chưa có món thì xóa đi
    // và trả bàn về trạng thái Trống.

    if (order != null &&
        order.OrderDetails.Count == 0)
    {
        _context.Orders.Remove(order);

        table.Status = "Empty";

        await _context.SaveChangesAsync();

        order = null;
    }


    // =========================================
    // BÀN CHƯA CÓ HÓA ĐƠN
    // =========================================
    // Chỉ mở bàn để xem thực đơn.
    // KHÔNG tạo hóa đơn.
    // KHÔNG đổi trạng thái bàn.

    if (order == null)
    {
        return Json(new
        {
            orderId = (int?)null,

            orderCode = "",

            tableId = table.Id,

            tableName = table.TableName,

            subtotal = 0,

            totalAmount = 0,

            items = Array.Empty<object>()
        });
    }
// Nếu bàn đã có hóa đơn Open và đã có món
// thì đảm bảo trạng thái bàn là Đang phục vụ
if (table.Status != "Serving")
{
    table.Status = "Serving";

    await _context.SaveChangesAsync();
}

    // =========================================
    // BÀN ĐÃ CÓ HÓA ĐƠN CHƯA THANH TOÁN
    // =========================================

    return Json(new
    {
        orderId = order.Id,

        orderCode = order.OrderCode,

        tableId = table.Id,

        tableName = table.TableName,

        subtotal = order.Subtotal,

        totalAmount = order.TotalAmount,

        items = order.OrderDetails.Select(x => new
        {
            foodId = x.FoodId,

            name = x.Food != null
                ? x.Food.Name
                : "",

            quantity = x.Quantity,

            price = x.UnitPrice,

            subtotal = x.Subtotal
        })
    });
}

        // =====================================================
// THÊM MÓN VÀO HÓA ĐƠN
// Chỉ khi chọn món đầu tiên mới tạo hóa đơn cho bàn
// =====================================================

[HttpPost]
public async Task<IActionResult> AddItem(
    int? orderId,
    int foodId,
    int? tableId)
{
    // =========================================
    // LẤY MÓN ĂN
    // =========================================

    var food = await _context.Foods
        .FirstOrDefaultAsync(f =>
            f.Id == foodId &&
            f.IsAvailable);

    if (food == null)
    {
        return NotFound("Không tìm thấy món.");
    }


    Order? order = null;


    // =========================================
    // NẾU BÀN ĐÃ CÓ HÓA ĐƠN
    // =========================================

    if (orderId.HasValue)
    {
        order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o =>
                o.Id == orderId.Value &&
                o.Status == "Open");
    }


    // =========================================
    // NẾU CHƯA CÓ HÓA ĐƠN
    // THÌ ĐÂY LÀ MÓN ĐẦU TIÊN CỦA BÀN
    // =========================================

    if (order == null)
    {
        if (!tableId.HasValue)
        {
            return BadRequest("Chưa chọn bàn.");
        }


        // Kiểm tra bàn có tồn tại không
        var table = await _context.RestaurantTables
            .FirstOrDefaultAsync(t =>
                t.Id == tableId.Value);

        if (table == null)
        {
            return NotFound("Không tìm thấy bàn.");
        }


        // Kiểm tra lại xem bàn đã có đơn Open chưa
        // để tránh tạo trùng hóa đơn
        order = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o =>
                o.TableId == tableId.Value &&
                o.OrderType == "DineIn" &&
                o.Status == "Open");


        // Nếu thực sự chưa có thì mới tạo
        if (order == null)
        {
            var userIdText = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdText, out int userId))
            {
                return Unauthorized();
            }


            order = new Order
            {
                // Tạo mã tạm để lấy ID
                OrderCode =
                    "TMP-" +
                    Guid.NewGuid().ToString("N"),

                UserId = userId,

                TableId = table.Id,

                OrderType = "DineIn",

                Status = "Open",

                Subtotal = 0,

                Discount = 0,

                TotalAmount = 0,

                CreatedAt = DateTime.Now
            };


            _context.Orders.Add(order);

            // Lưu để lấy ID hóa đơn
            await _context.SaveChangesAsync();


            // Tạo mã hóa đơn chính thức
            order.OrderCode =
                $"HD{order.Id:D6}";
        }


        // Chỉ lúc khách thực sự gọi món
        // bàn mới chuyển sang Đang phục vụ
        table.Status = "Serving";
    }


    // =========================================
    // KIỂM TRA MÓN ĐÃ CÓ TRONG HÓA ĐƠN CHƯA
    // =========================================

    var detail = order.OrderDetails
        .FirstOrDefault(x =>
            x.FoodId == foodId);


    if (detail == null)
    {
        detail = new OrderDetail
        {
            OrderId = order.Id,

            FoodId = food.Id,

            Quantity = 1,

            UnitPrice = food.Price,

            Subtotal = food.Price
        };

        _context.OrderDetails.Add(detail);
    }
    else
    {
        // Nếu món đã có thì tăng số lượng
        detail.Quantity++;

        detail.Subtotal =
            detail.Quantity *
            detail.UnitPrice;
    }


    // Tính lại tổng tiền hóa đơn
    await UpdateOrderTotal(order);

    await _context.SaveChangesAsync();


    // Trả ID hóa đơn về JavaScript
    // để những lần bấm món tiếp theo sử dụng lại đơn này
    return Json(new
    {
        success = true,

        orderId = order.Id,

        orderCode = order.OrderCode,

        tableStatus = "Serving",

        subtotal = order.Subtotal,

        totalAmount = order.TotalAmount
    });
}

        // =====================================================
        // TĂNG HOẶC GIẢM SỐ LƯỢNG MÓN
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(
            int orderId,
            int foodId,
            int change)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.Status == "Open");

            if (order == null)
            {
                return NotFound();
            }

            var detail = order.OrderDetails
                .FirstOrDefault(x => x.FoodId == foodId);

            if (detail == null)
            {
                return NotFound();
            }

            detail.Quantity += change;

            // Nếu số lượng bằng 0 thì xóa món khỏi hóa đơn
            if (detail.Quantity <= 0)
            {
                _context.OrderDetails.Remove(detail);
            }
            else
            {
                detail.Subtotal =
                    detail.Quantity * detail.UnitPrice;
            }

            await UpdateOrderTotal(order);

            await _context.SaveChangesAsync();

            return Ok();
        }


        // =====================================================
        // XÓA MÓN KHỎI HÓA ĐƠN
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> RemoveItem(
            int orderId,
            int foodId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o =>
                    o.Id == orderId &&
                    o.Status == "Open");

            if (order == null)
            {
                return NotFound();
            }

            var detail = order.OrderDetails
                .FirstOrDefault(x => x.FoodId == foodId);

            if (detail == null)
            {
                return NotFound();
            }

           // Xóa món khỏi hóa đơn
_context.OrderDetails.Remove(detail);

// Xóa luôn món khỏi danh sách đang dùng để tính tổng
order.OrderDetails.Remove(detail);

// Tính lại tổng tiền
await UpdateOrderTotal(order);

await _context.SaveChangesAsync();
            return Ok();
        }

        // =====================================================
// THANH TOÁN HÓA ĐƠN ĂN TẠI QUÁN
// =====================================================

[HttpPost]
public async Task<IActionResult> Checkout(
    int orderId,
    string paymentMethod)
{
    // Chỉ chấp nhận 2 phương thức thanh toán của quán
    if (paymentMethod != "Cash" &&
        paymentMethod != "BankTransfer")
    {
        return BadRequest("Phương thức thanh toán không hợp lệ.");
    }

    // Lấy hóa đơn đang mở
    var order = await _context.Orders
        .Include(o => o.OrderDetails)
        .Include(o => o.Table)
        .Include(o => o.Payment)
        .FirstOrDefaultAsync(o =>
            o.Id == orderId &&
            o.Status == "Open");

    if (order == null)
    {
        return NotFound("Không tìm thấy hóa đơn.");
    }

    // Không cho thanh toán hóa đơn chưa có món
    if (order.OrderDetails.Count == 0)
    {
        return BadRequest("Hóa đơn chưa có món.");
    }

    // Tính lại tổng tiền trước khi thanh toán
    order.Subtotal = order.OrderDetails
        .Sum(x => x.Quantity * x.UnitPrice);

    order.TotalAmount =
        order.Subtotal - order.Discount;

    if (order.TotalAmount < 0)
    {
        order.TotalAmount = 0;
    }

    // Thời điểm thanh toán
    var paidAt = DateTime.Now;

    // =========================================
    // TẠO THÔNG TIN THANH TOÁN
    // =========================================

    var payment = new Payment
    {
        OrderId = order.Id,

        PaymentMethod = paymentMethod,

        Amount = order.TotalAmount,

        PaidAt = paidAt
    };

    _context.Payments.Add(payment);


    // =========================================
    // ĐÁNH DẤU HÓA ĐƠN ĐÃ THANH TOÁN
    // =========================================

    order.Status = "Paid";

    order.PaidAt = paidAt;


    // =========================================
    // NẾU LÀ ĐƠN TẠI BÀN
    // THÌ TRẢ BÀN VỀ TRẠNG THÁI TRỐNG
    // =========================================

    if (order.OrderType == "DineIn" &&
        order.Table != null)
    {
        order.Table.Status = "Empty";
    }


    await _context.SaveChangesAsync();


    // Trả thông tin về giao diện
    return Json(new
    {
        success = true,

        orderId = order.Id,

        orderCode = order.OrderCode,

        totalAmount = order.TotalAmount,

        paymentMethod = payment.PaymentMethod
    });
}

// =====================================================
// THANH TOÁN ĐƠN MANG ĐI
// Đơn mang đi được tạo và thanh toán ngay
// =====================================================

[HttpPost]
public async Task<IActionResult> CheckoutTakeAway(
    [FromBody] TakeAwayCheckoutRequest request)
{
    // Chỉ chấp nhận hai phương thức thanh toán
    if (request.PaymentMethod != "Cash" &&
        request.PaymentMethod != "BankTransfer")
    {
        return BadRequest(
            "Phương thức thanh toán không hợp lệ."
        );
    }

    // Không cho thanh toán nếu chưa có món
    if (request.Items == null ||
        request.Items.Count == 0)
    {
        return BadRequest(
            "Đơn mang đi chưa có món."
        );
    }

    // Lấy ID tài khoản đang đăng nhập
    var userIdText = User.FindFirstValue(
        ClaimTypes.NameIdentifier
    );

    if (!int.TryParse(userIdText, out int userId))
    {
        return Unauthorized();
    }

    // Bắt đầu transaction để tránh lưu dở hóa đơn
    await using var transaction =
        await _context.Database.BeginTransactionAsync();

    try
    {
        // =========================================
        // TẠO HÓA ĐƠN MANG ĐI
        // =========================================

        var order = new Order
        {
            // Tạo mã tạm để lấy ID trước
            OrderCode =
                "TMP-" +
                Guid.NewGuid().ToString("N"),

            UserId = userId,

            // Mang đi không có bàn
            TableId = null,

            OrderType = "TakeAway",

            Status = "Open",

            Subtotal = 0,

            Discount = 0,

            TotalAmount = 0,

            CreatedAt = DateTime.Now
        };

        _context.Orders.Add(order);

        await _context.SaveChangesAsync();


        // Tạo mã hóa đơn chính thức
        order.OrderCode =
            $"HD{order.Id:D6}";


        decimal subtotal = 0;


        // =========================================
        // LƯU CÁC MÓN TRONG HÓA ĐƠN
        // =========================================

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                continue;
            }

            var food = await _context.Foods
                .FirstOrDefaultAsync(f =>
                    f.Id == item.FoodId &&
                    f.IsAvailable);

            if (food == null)
            {
                await transaction.RollbackAsync();

                return BadRequest(
                    $"Không tìm thấy món có ID {item.FoodId}."
                );
            }

            var itemSubtotal =
                food.Price * item.Quantity;

            subtotal += itemSubtotal;

            var detail =
                new OrderDetail
                {
                    OrderId = order.Id,

                    FoodId = food.Id,

                    Quantity =
                        item.Quantity,

                    UnitPrice =
                        food.Price,

                    Subtotal =
                        itemSubtotal
                };

            _context.OrderDetails.Add(
                detail
            );
        }


        // =========================================
        // TÍNH TỔNG TIỀN
        // =========================================

        order.Subtotal =
            subtotal;

        order.TotalAmount =
            subtotal - order.Discount;

        if (order.TotalAmount < 0)
        {
            order.TotalAmount = 0;
        }


        var paidAt =
            DateTime.Now;


        // =========================================
        // TẠO THANH TOÁN
        // =========================================

        var payment =
            new Payment
            {
                OrderId =
                    order.Id,

                PaymentMethod =
                    request.PaymentMethod,

                Amount =
                    order.TotalAmount,

                PaidAt =
                    paidAt
            };

        _context.Payments.Add(
            payment
        );


        // =========================================
        // ĐÁNH DẤU HÓA ĐƠN ĐÃ THANH TOÁN
        // =========================================

        order.Status =
            "Paid";

        order.PaidAt =
            paidAt;


        await _context.SaveChangesAsync();

        await transaction.CommitAsync();


        // Trả thông tin về giao diện
        return Json(new
        {
            success = true,

            orderId =
                order.Id,

            orderCode =
                order.OrderCode,

            totalAmount =
                order.TotalAmount,

            paymentMethod =
                payment.PaymentMethod
        });
    }
    catch
    {
        await transaction.RollbackAsync();

        return StatusCode(
            500,
            "Có lỗi khi thanh toán đơn mang đi."
        );
    }
}

// =====================================================
// HIỂN THỊ HÓA ĐƠN ĐỂ IN
// =====================================================

[HttpGet]
public async Task<IActionResult> Invoice(int id)
{
    // Lấy đầy đủ thông tin hóa đơn
    var order = await _context.Orders
        .Include(o => o.Table)
        .Include(o => o.User)
        .Include(o => o.Payment)
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Food)
        .FirstOrDefaultAsync(o => o.Id == id);

    if (order == null)
    {
        return NotFound();
    }

    return View(order);
}


        // =====================================================
        // TÍNH LẠI TỔNG TIỀN HÓA ĐƠN
        // =====================================================

        private Task UpdateOrderTotal(Order order)
        {
            order.Subtotal =
                order.OrderDetails
                    .Where(x => x.Quantity > 0)
                    .Sum(x => x.Quantity * x.UnitPrice);

            order.TotalAmount =
                order.Subtotal - order.Discount;

            if (order.TotalAmount < 0)
            {
                order.TotalAmount = 0;
            }

            return Task.CompletedTask;
        }
    }
}