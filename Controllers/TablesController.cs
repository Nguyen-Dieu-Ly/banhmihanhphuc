using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;
using QRCoder;

namespace banhmihanhphuc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TablesController : Controller
    {
        private readonly AppDbContext _context;


        // =========================================
        // KẾT NỐI VỚI CƠ SỞ DỮ LIỆU
        // =========================================

        public TablesController(AppDbContext context)
        {
            _context = context;
        }


        // =========================================
        // HIỂN THỊ DANH SÁCH BÀN
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tables =
                await _context.RestaurantTables

                    .OrderBy(t => t.Id)

                    .ToListAsync();


            return View(tables);
        }


        // =========================================
        // HIỂN THỊ FORM THÊM BÀN
        // =========================================

        [HttpGet]
        public IActionResult Create()
        {
            var table =
                new RestaurantTable
                {
                    Capacity = 4,

                    Status = "Empty"
                };


            return View(table);
        }


        // =========================================
        // LƯU BÀN MỚI
        // =========================================

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(
            RestaurantTable model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // Xóa khoảng trắng thừa
            model.TableName =
                model.TableName?.Trim()
                ?? string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    model.TableName
                )
            )
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn không được để trống."
                );


                return View(model);
            }


            if (model.Capacity <= 0)
            {
                ModelState.AddModelError(
                    "Capacity",
                    "Sức chứa phải lớn hơn 0."
                );


                return View(model);
            }


            // Kiểm tra tên bàn đã tồn tại chưa
            var existed =
                await _context.RestaurantTables

                    .AnyAsync(t =>
                        t.TableName ==
                        model.TableName);


            if (existed)
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn này đã tồn tại."
                );


                return View(model);
            }


            // Bàn mới mặc định là trống
            model.Status =
                "Empty";


            _context.RestaurantTables
                .Add(model);


            await _context
                .SaveChangesAsync();


            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // HIỂN THỊ FORM SỬA BÀN
        // =========================================

        [HttpGet]

        public async Task<IActionResult> Edit(
            int id)
        {
            var table =
                await _context.RestaurantTables

                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (table == null)
            {
                return NotFound();
            }


            return View(table);
        }


        // =========================================
        // LƯU THAY ĐỔI BÀN
        // =========================================

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(
            int id,
            RestaurantTable model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            model.TableName =
                model.TableName?.Trim()
                ?? string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    model.TableName
                )
            )
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn không được để trống."
                );


                return View(model);
            }


            if (model.Capacity <= 0)
            {
                ModelState.AddModelError(
                    "Capacity",
                    "Sức chứa phải lớn hơn 0."
                );


                return View(model);
            }


            var table =
                await _context.RestaurantTables

                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (table == null)
            {
                return NotFound();
            }


            // Kiểm tra tên bàn có bị trùng
            // với bàn khác hay không
            var existed =
                await _context.RestaurantTables

                    .AnyAsync(t =>
                        t.TableName ==
                            model.TableName &&

                        t.Id != id);


            if (existed)
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn này đã tồn tại."
                );


                return View(model);
            }


            table.TableName =
                model.TableName;


            table.Capacity =
                model.Capacity;


            // Không tự thay đổi trạng thái bàn
            // vì trạng thái Empty / Serving
            // đang được đồng bộ từ phần bán hàng


            await _context
                .SaveChangesAsync();


            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // HIỂN THỊ XÁC NHẬN XÓA BÀN
        // =========================================

        [HttpGet]

        public async Task<IActionResult> Delete(
            int id)
        {
            var table =
                await _context.RestaurantTables

                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (table == null)
            {
                return NotFound();
            }


            return View(table);
        }


        // =========================================
        // XỬ LÝ XÓA BÀN
        // =========================================

        [HttpPost]

        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var table =
                await _context.RestaurantTables

                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (table == null)
            {
                return NotFound();
            }


            // Không cho xóa bàn đang phục vụ
            if (table.Status == "Serving")
            {
                TempData["Error"] =
                    "Không thể xóa bàn đang phục vụ.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            // Kiểm tra bàn có yêu cầu QR order
            // đang chờ xử lý hay không
            var hasPendingQrOrder =
                await _context.CustomerOrderRequests

                    .AnyAsync(r =>
                        r.TableId == id &&
                        r.Status == "Pending");


            if (hasPendingQrOrder)
            {
                TempData["Error"] =
                    "Không thể xóa bàn đang có yêu cầu gọi món từ QR.";


                return RedirectToAction(
                    nameof(Index)
                );
            }


            _context.RestaurantTables
                .Remove(table);


            await _context
                .SaveChangesAsync();


            return RedirectToAction(
                nameof(Index)
            );
        }


        // =========================================
        // HIỂN THỊ QR ORDER CỦA BÀN
        // =========================================

        [HttpGet]

        public async Task<IActionResult> QR(
            int id)
        {
            var table =
                await _context.RestaurantTables

                    .FirstOrDefaultAsync(t =>
                        t.Id == id);


            if (table == null)
            {
                return NotFound();
            }


            // =========================================
            // TẠO ĐƯỜNG DẪN ORDER RIÊNG CHO BÀN
            //
            // Local:
            // http://localhost:xxxx/CustomerOrder/Table/1
            //
            // Online:
            // https://banhmihanhphuc.onrender.com/
            // CustomerOrder/Table/1
            // =========================================

            var orderUrl =
                $"{Request.Scheme}://" +
                $"{Request.Host}" +
                $"/CustomerOrder/Table/{table.Id}";


            // =========================================
            // TẠO MÃ QR
            // =========================================

            using var qrGenerator =
                new QRCodeGenerator();


            using var qrData =
                qrGenerator.CreateQrCode(
                    orderUrl,
                    QRCodeGenerator.ECCLevel.Q
                );


            var qrCode =
                new PngByteQRCode(
                    qrData
                );


            var qrBytes =
                qrCode.GetGraphic(
                    20
                );


            // =========================================
            // CHUYỂN QR THÀNH BASE64
            // ĐỂ HIỂN THỊ TRỰC TIẾP TRÊN VIEW
            // =========================================

            ViewBag.QrImage =
                "data:image/png;base64," +
                Convert.ToBase64String(
                    qrBytes
                );


            ViewBag.OrderUrl =
                orderUrl;


            return View(table);
        }
    }
}