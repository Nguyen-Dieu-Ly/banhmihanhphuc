using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;

namespace banhmihanhphuc.Controllers
{
    public class TestController : Controller
    {
        private readonly AppDbContext _context;

        public TestController(AppDbContext context)
        {
            _context = context;
        }

        // Kiểm tra kết nối và lấy danh sách món ăn
        public async Task<IActionResult> Index()
        {
            var foods = await _context.Foods.ToListAsync();

            return Json(foods);
        }

        // Tạo mật khẩu đã mã hóa để lưu vào cơ sở dữ liệu
        public IActionResult CreatePasswordHash()
        {
            var hash = BCrypt.Net.BCrypt.HashPassword("123456");

            return Content(hash);
        }
    }
}