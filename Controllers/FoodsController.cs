using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;
using System.Net.Http.Headers;

namespace banhmihanhphuc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FoodsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // Kết nối cơ sở dữ liệu và cấu hình Supabase
        public FoodsController(
            AppDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // =========================================
        // HIỂN THỊ DANH SÁCH MÓN ĂN
        // =========================================
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId)
        {
            var query = _context.Foods
                .Include(f => f.Category)
                .AsQueryable();

            // Tìm kiếm món theo tên
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(f =>
                    f.Name.ToLower()
                        .Contains(search.ToLower())
                );
            }

            // Lọc món theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(f =>
                    f.CategoryId == categoryId.Value
                );
            }

            var foods = await query
                .OrderBy(f => f.CategoryId)
                .ThenBy(f => f.Name)
                .ToListAsync();

            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(foods);
        }


        // =========================================
        // HIỂN THỊ TRANG THÊM MÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

            return View();
        }


        // =========================================
        // LƯU MÓN ĂN MỚI
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Food model,
            IFormFile? imageFile)
        {
            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();


            // Kiểm tra tên món bị trùng
            var existed = await _context.Foods
                .AnyAsync(f =>
                    f.Name.ToLower() ==
                    model.Name.ToLower());

            if (existed)
            {
                ModelState.AddModelError(
                    "Name",
                    "Tên món này đã tồn tại."
                );
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =========================================
            // UPLOAD ẢNH LÊN SUPABASE STORAGE
            // =========================================
            if (imageFile != null &&
                imageFile.Length > 0)
            {
                var imageUrl =
                    await UploadImageToSupabase(imageFile);

                if (imageUrl == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể tải ảnh lên Supabase."
                    );

                    return View(model);
                }

                model.ImageUrl = imageUrl;
            }


            // Món mới mặc định đang bán
            model.IsAvailable = true;


            _context.Foods.Add(model);

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // HIỂN THỊ TRANG SỬA MÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
            {
                return NotFound();
            }

            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

            return View(food);
        }


        // =========================================
        // LƯU THÔNG TIN MÓN SAU KHI SỬA
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Food model,
            IFormFile? imageFile)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
            {
                return NotFound();
            }


            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();


            // Kiểm tra tên món có bị trùng
            var existed = await _context.Foods
                .AnyAsync(f =>
                    f.Id != id &&
                    f.Name.ToLower() ==
                    model.Name.ToLower()
                );

            if (existed)
            {
                ModelState.AddModelError(
                    "Name",
                    "Tên món này đã tồn tại."
                );
            }


            if (!ModelState.IsValid)
            {
                model.ImageUrl = food.ImageUrl;

                return View(model);
            }


            // =========================================
            // CẬP NHẬT THÔNG TIN MÓN
            // =========================================
            food.Name = model.Name;

            food.CategoryId = model.CategoryId;

            food.Price = model.Price;

            food.IsAvailable = model.IsAvailable;


            // =========================================
            // NẾU CHỌN ẢNH MỚI
            // =========================================
            if (imageFile != null &&
                imageFile.Length > 0)
            {
                var imageUrl =
                    await UploadImageToSupabase(imageFile);

                if (imageUrl == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể tải ảnh lên Supabase."
                    );

                    model.ImageUrl = food.ImageUrl;

                    return View(model);
                }


                // Lưu URL ảnh mới
                food.ImageUrl = imageUrl;
            }


            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // HIỂN THỊ XÁC NHẬN XÓA MÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var food = await _context.Foods
                .Include(f => f.Category)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
            {
                return NotFound();
            }

            return View(food);
        }


        // =========================================
        // XỬ LÝ XÓA HOẶC NGỪNG BÁN MÓN
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var food = await _context.Foods
                .FirstOrDefaultAsync(f => f.Id == id);

            if (food == null)
            {
                return NotFound();
            }


            var hasOrderDetails =
                await _context.OrderDetails
                    .AnyAsync(od =>
                        od.FoodId == id);


            if (hasOrderDetails)
            {
                // Đã có lịch sử hóa đơn
                // nên chỉ chuyển sang ngừng bán
                food.IsAvailable = false;

                await _context.SaveChangesAsync();

                TempData["Message"] =
                    "Món đã có trong lịch sử hóa đơn nên hệ thống chuyển sang trạng thái Ngừng bán.";
            }
            else
            {
                // Chưa có hóa đơn thì xóa hẳn
                _context.Foods.Remove(food);

                await _context.SaveChangesAsync();

                TempData["Message"] =
                    "Đã xóa món thành công.";
            }


            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // UPLOAD ẢNH LÊN SUPABASE STORAGE
        // =========================================
        private async Task<string?> UploadImageToSupabase(
            IFormFile imageFile)
        {
            // Kiểm tra đuôi ảnh
            var extension =
                Path.GetExtension(
                    imageFile.FileName
                ).ToLowerInvariant();


            var allowedExtensions =
                new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };


            if (!allowedExtensions.Contains(extension))
            {
                return null;
            }


            // Lấy cấu hình từ Render Environment
            var supabaseUrl =
                _configuration["Supabase:Url"];

            var serviceKey =
                _configuration["Supabase:ServiceKey"];


            if (string.IsNullOrWhiteSpace(supabaseUrl) ||
                string.IsNullOrWhiteSpace(serviceKey))
            {
                return null;
            }


            // Tạo tên ảnh không bị trùng
            var fileName =
                Guid.NewGuid()
                    .ToString("N")
                + extension;


            // Bucket đã tạo trên Supabase
            const string bucketName =
                "food-images";


            var uploadUrl =
                $"{supabaseUrl.TrimEnd('/')}" +
                $"/storage/v1/object/" +
                $"{bucketName}/" +
                $"{fileName}";


            using var httpClient =
                new HttpClient();


            // Quyền truy cập Supabase
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    serviceKey
                );


            httpClient.DefaultRequestHeaders.Add(
                "apikey",
                serviceKey
            );


            // Đọc file ảnh
            using var stream =
                imageFile.OpenReadStream();


            using var content =
                new StreamContent(stream);


            content.Headers.ContentType =
                new MediaTypeHeaderValue(
                    imageFile.ContentType
                );


            // Cho phép ghi file lên Storage
            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    uploadUrl
                );


            request.Headers.Add(
                "x-upsert",
                "true"
            );


            request.Content = content;


            var response =
                await httpClient.SendAsync(request);


            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                Console.WriteLine(
                    "Supabase upload error: "
                    + error
                );

                return null;
            }


            // URL public để hiển thị ảnh
            var publicUrl =
                $"{supabaseUrl.TrimEnd('/')}" +
                $"/storage/v1/object/public/" +
                $"{bucketName}/" +
                $"{fileName}";


            return publicUrl;
        }
    }
}