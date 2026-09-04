using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;

namespace banhmihanhphuc.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // DANH SÁCH HÓA ĐƠN + TÌM KIẾM + LỌC NGÀY
        // =====================================================
        public async Task<IActionResult> Index(
            string? search,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // Chỉ lấy hóa đơn đã thanh toán
            var query = _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Payment)
                .Where(o => o.Status == "Paid")
                .AsQueryable();


            // =========================================
            // TÌM THEO MÃ HÓA ĐƠN
            // =========================================
            if (!string.IsNullOrWhiteSpace(search))
            {
                string keyword =
                    search.Trim().ToLower();

                query = query.Where(o =>
                    o.OrderCode.ToLower()
                        .Contains(keyword)
                );
            }


            // Lấy dữ liệu trước
            // rồi lọc ngày trong C# để tránh lỗi DateTime PostgreSQL
            var invoices = await query
                .OrderByDescending(o => o.PaidAt)
                .ToListAsync();


            // =========================================
            // LỌC TỪ NGÀY
            // =========================================
            if (fromDate.HasValue)
            {
                DateTime start =
                    fromDate.Value.Date;

                invoices = invoices
                    .Where(o =>
                        o.PaidAt.HasValue &&
                        o.PaidAt.Value.Date >= start
                    )
                    .ToList();
            }


            // =========================================
            // LỌC ĐẾN NGÀY
            // =========================================
            if (toDate.HasValue)
            {
                DateTime end =
                    toDate.Value.Date;

                invoices = invoices
                    .Where(o =>
                        o.PaidAt.HasValue &&
                        o.PaidAt.Value.Date <= end
                    )
                    .ToList();
            }


            // Giữ lại dữ liệu người dùng đã nhập
            ViewBag.Search = search;

            ViewBag.FromDate =
                fromDate?.ToString("yyyy-MM-dd");

            ViewBag.ToDate =
                toDate?.ToString("yyyy-MM-dd");


            return View(invoices);
        }


        // =====================================================
        // XEM CHI TIẾT HÓA ĐƠN
        // =====================================================
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Payment)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Food)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.Status == "Paid"
                );

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }
    }
}