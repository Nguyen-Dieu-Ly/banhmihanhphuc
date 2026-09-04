using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TablesController : Controller
    {
        private readonly AppDbContext _context;

        // Kết nối với cơ sở dữ liệu
        public TablesController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // HIỂN THỊ DANH SÁCH BÀN
        // =========================================
        public async Task<IActionResult> Index()
        {
            var tables = await _context.RestaurantTables
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
            var table = new RestaurantTable
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

            // Kiểm tra tên bàn đã tồn tại chưa
            var existed = await _context.RestaurantTables
                .AnyAsync(t => t.TableName == model.TableName);

            if (existed)
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn này đã tồn tại."
                );

                return View(model);
            }

            model.Status = "Empty";

            _context.RestaurantTables.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // HIỂN THỊ FORM SỬA BÀN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t => t.Id == id);

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

            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            // Kiểm tra tên bàn có bị trùng với bàn khác không
            var existed = await _context.RestaurantTables
                .AnyAsync(t =>
                    t.TableName == model.TableName &&
                    t.Id != id
                );

            if (existed)
            {
                ModelState.AddModelError(
                    "TableName",
                    "Tên bàn này đã tồn tại."
                );

                return View(model);
            }

            table.TableName = model.TableName;
            table.Capacity = model.Capacity;

            // Không tự sửa trạng thái Serving/Empty ở đây
            // vì trạng thái đang được đồng bộ với phần bán hàng

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // HIỂN THỊ XÁC NHẬN XÓA BÀN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            return View(table);
        }


        // =========================================
        // XÓA BÀN
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var table = await _context.RestaurantTables
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound();
            }

            // Không cho xóa bàn đang phục vụ
            if (table.Status == "Serving")
            {
                TempData["Error"] =
                    "Không thể xóa bàn đang phục vụ.";

                return RedirectToAction(nameof(Index));
            }

            _context.RestaurantTables.Remove(table);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}