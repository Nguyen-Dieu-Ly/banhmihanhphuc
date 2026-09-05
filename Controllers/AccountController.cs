using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Username == model.Username &&
                    u.IsActive);

            if (user == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View(model);
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(
                model.Password,
                user.PasswordHash
            );

            if (!passwordOk)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var userRole in user.UserRoles)
            {
                if (userRole.Role != null)
                {
                    claims.Add(
                        new Claim(
                            ClaimTypes.Role,
                            userRole.Role.Name
                        )
                    );
                }
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

    
        [HttpGet]
public IActionResult AccessDenied()
{
    return View();
}
// =========================================
// QUÊN MẬT KHẨU
// =========================================

[HttpGet]
public IActionResult ForgotPassword()
{
    return View();
}


[HttpPost]
public async Task<IActionResult> ForgotPassword(
    string username,
    string newPassword,
    string confirmPassword)
{
    // Kiểm tra nhập đủ
    if (string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(newPassword) ||
        string.IsNullOrWhiteSpace(confirmPassword))
    {
        ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
        return View();
    }


    // Kiểm tra mật khẩu tối thiểu
    if (newPassword.Length < 6)
    {
        ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự.";
        return View();
    }


    // Kiểm tra xác nhận mật khẩu
    if (newPassword != confirmPassword)
    {
        ViewBag.Error = "Mật khẩu xác nhận không khớp.";
        return View();
    }


    // Tìm tài khoản
    var user = await _context.Users
        .FirstOrDefaultAsync(u =>
            u.Username == username.Trim() &&
            u.IsActive
        );


    if (user == null)
    {
        ViewBag.Error = "Không tìm thấy tài khoản.";
        return View();
    }


    // Hash mật khẩu mới
    user.PasswordHash =
        BCrypt.Net.BCrypt.HashPassword(newPassword);


    await _context.SaveChangesAsync();


    TempData["Success"] =
        "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại.";


    return RedirectToAction("Login");
}
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            return RedirectToAction(
                "Login",
                "Account"
            );
        }
    }
}