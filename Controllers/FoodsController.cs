using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;
using System.IO;

namespace banhmihanhphuc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FoodsController : Controller
    {
        private readonly AppDbContext _context;

        // Kết nối với cơ sở dữ liệu
        public FoodsController(AppDbContext context)
        {
            _context = context;
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

            // Tìm kiếm món ăn theo tên
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

            // Lấy danh mục để hiển thị ở bộ lọc
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
            // Lấy danh sách danh mục
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
            // Lấy lại danh mục để nếu có lỗi
            // thì form vẫn hiển thị danh sách danh mục
            ViewBag.Categories =
                await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();


            // =========================================
            // KIỂM TRA TÊN MÓN
            // =========================================
            var existed = await _context.Foods
                .AnyAsync(f => f.Name.ToLower()
                    == model.Name.ToLower());

            if (existed)
            {
                ModelState.AddModelError(
                    "Name",
                    "Tên món này đã tồn tại."
                );
            }


            // Nếu dữ liệu không hợp lệ
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =========================================
            // XỬ LÝ ẢNH MÓN ĂN
            // =========================================
            if (imageFile != null &&
                imageFile.Length > 0)
            {
                // Lấy đuôi file ảnh
                var extension =
                    Path.GetExtension(
                        imageFile.FileName
                    ).ToLower();

                // Các định dạng ảnh cho phép
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
                    ModelState.AddModelError(
                        "",
                        "Ảnh chỉ được dùng JPG, JPEG, PNG hoặc WEBP."
                    );

                    return View(model);
                }


                // =========================================
                // TẠO THƯ MỤC LƯU ẢNH
                // =========================================
                var folderPath =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "images",
                        "foods"
                    );

                // Nếu thư mục chưa tồn tại thì tự tạo
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(
                        folderPath
                    );
                }


                // =========================================
                // TẠO TÊN FILE ẢNH KHÔNG BỊ TRÙNG
                // =========================================
                var fileName =
                    Guid.NewGuid()
                        .ToString("N")
                    + extension;


                var filePath =
                    Path.Combine(
                        folderPath,
                        fileName
                    );


                // =========================================
                // LƯU ẢNH VÀO WWWROOT
                // =========================================
                using (var stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    await imageFile
                        .CopyToAsync(stream);
                }


                // =========================================
                // LƯU ĐƯỜNG DẪN ẢNH VÀO DATABASE
                // =========================================
                model.ImageUrl =
                    "/images/foods/"
                    + fileName;
            }


            // =========================================
            // TRẠNG THÁI MÓN
            // =========================================

            // Món mới mặc định được bán
            model.IsAvailable = true;


            // =========================================
            // LƯU VÀO DATABASE
            // =========================================
            _context.Foods.Add(model);

            await _context.SaveChangesAsync();


            // Sau khi thêm xong quay về danh sách món
            return RedirectToAction(
                nameof(Index)
            );
        }
    // =========================================
// HIỂN THỊ TRANG SỬA MÓN
// =========================================
[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    // Tìm món cần sửa
    var food = await _context.Foods
        .FirstOrDefaultAsync(f => f.Id == id);

    if (food == null)
    {
        return NotFound();
    }

    // Lấy danh sách danh mục
    ViewBag.Categories = await _context.Categories
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
    // Tìm món hiện tại trong database
    var food = await _context.Foods
        .FirstOrDefaultAsync(f => f.Id == id);

    if (food == null)
    {
        return NotFound();
    }


    // Lấy danh mục để hiển thị lại nếu có lỗi
    ViewBag.Categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();


    // Kiểm tra tên món có bị trùng với món khác không
    var existed = await _context.Foods
        .AnyAsync(f =>
            f.Id != id &&
            f.Name.ToLower() == model.Name.ToLower()
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
        // Giữ lại ảnh cũ khi form có lỗi
        model.ImageUrl = food.ImageUrl;

        return View(model);
    }


    // =========================================
    // CẬP NHẬT THÔNG TIN
    // =========================================

    food.Name = model.Name;

    food.CategoryId = model.CategoryId;

    food.Price = model.Price;

    food.IsAvailable = model.IsAvailable;


    // =========================================
    // NẾU NGƯỜI DÙNG CHỌN ẢNH MỚI
    // =========================================
    if (imageFile != null &&
        imageFile.Length > 0)
    {
        var extension =
            Path.GetExtension(
                imageFile.FileName
            ).ToLower();


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
            ModelState.AddModelError(
                "",
                "Ảnh chỉ được dùng JPG, JPEG, PNG hoặc WEBP."
            );

            model.ImageUrl = food.ImageUrl;

            return View(model);
        }


        // Thư mục chứa ảnh món
        var folderPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "foods"
            );


        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(
                folderPath
            );
        }


        // Tạo tên ảnh mới
        var fileName =
            Guid.NewGuid().ToString("N")
            + extension;


        var filePath =
            Path.Combine(
                folderPath,
                fileName
            );


        // Lưu ảnh mới
        using (var stream =
               new FileStream(
                   filePath,
                   FileMode.Create))
        {
            await imageFile
                .CopyToAsync(stream);
        }


        // Lưu đường dẫn ảnh mới
        food.ImageUrl =
            "/images/foods/" +
            fileName;
    }


    // =========================================
    // LƯU DATABASE
    // =========================================

    await _context.SaveChangesAsync();


    return RedirectToAction(
        nameof(Index)
    );
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
// XỬ LÝ XÓA MÓN
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

    // Kiểm tra món đã từng xuất hiện trong hóa đơn chưa
    var hasOrderDetails = await _context.OrderDetails
        .AnyAsync(od => od.FoodId == id);

    if (hasOrderDetails)
    {
        // Nếu đã có lịch sử hóa đơn thì không xóa khỏi database
        // mà chỉ chuyển sang trạng thái ngừng bán
        food.IsAvailable = false;

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Món đã có trong lịch sử hóa đơn nên hệ thống chuyển sang trạng thái Ngừng bán.";
    }
    else
    {
        // Nếu món chưa từng xuất hiện trong hóa đơn thì cho phép xóa hẳn
        _context.Foods.Remove(food);

        await _context.SaveChangesAsync();

        TempData["Message"] =
            "Đã xóa món thành công.";
    }

    return RedirectToAction(nameof(Index));
}
}
}