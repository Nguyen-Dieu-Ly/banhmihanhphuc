using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    [AllowAnonymous]
    public class CustomerOrderController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerOrderController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // KHÁCH QUÉT QR VÀ MỞ MENU CỦA BÀN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Table(int id)
        {
            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            var foods = await _context.Foods
                .Include(f => f.Category)
                .Where(f => f.IsAvailable)
                .OrderBy(f => f.CategoryId)
                .ThenBy(f => f.Name)
                .ToListAsync();

            ViewBag.Table = table;

            return View(foods);
        }

        // =========================================
        // KHÁCH GỬI ORDER ĐẾN POS
        // =========================================
        [HttpPost]
        public async Task<IActionResult> Submit(
            [FromBody] CustomerOrderDto request)
        {
            if (request.TableId <= 0)
            {
                return BadRequest("Không xác định được bàn.");
            }

            if (request.Items == null ||
                request.Items.Count == 0)
            {
                return BadRequest(
                    "Vui lòng chọn ít nhất một món."
                );
            }

            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t =>
                    t.Id == request.TableId);

            if (table == null)
            {
                return NotFound(
                    "Không tìm thấy bàn."
                );
            }

            var customerRequest =
    new CustomerOrderRequest
    {
        TableId = request.TableId,

        Status = "Pending",

        Note = string.IsNullOrWhiteSpace(
            request.Note
        )
            ? null
            : request.Note.Trim(),

        CreatedAt = DateTime.Now
    };

            _context.CustomerOrderRequests
                .Add(customerRequest);

            await _context.SaveChangesAsync();

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
                    continue;
                }

                var detail =
                    new CustomerOrderRequestDetail
                    {
                        CustomerOrderRequestId =
                            customerRequest.Id,

                        FoodId = food.Id,

                        Quantity = item.Quantity
                    };

                _context.CustomerOrderRequestDetails
                    .Add(detail);
            }

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,

                message =
                    "Đã gửi yêu cầu gọi món đến quầy."
            });
        }
    }
}