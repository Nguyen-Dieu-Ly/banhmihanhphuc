using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // =========================================
        // KẾT NỐI CƠ SỞ DỮ LIỆU
        // =========================================
        public HomeController(AppDbContext context)
        {
            _context = context;
        }


        // =========================================
        // TRANG TỔNG QUAN
        // =========================================
        public async Task<IActionResult> Index()
        {
            // =========================================
            // THỜI GIAN
            // =========================================

            var today =
                DateTime.Today;

            var sevenDaysAgo =
                today.AddDays(-6);


            // =========================================
            // LẤY CÁC HÓA ĐƠN ĐÃ THANH TOÁN
            // =========================================

            var paidOrders =
                await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food)
                    .Where(o =>
                        o.Status == "Paid" &&
                        o.PaidAt.HasValue
                    )
                    .ToListAsync();


            // =========================================
            // HÓA ĐƠN HÔM NAY
            // =========================================

            var todayOrders =
                paidOrders
                    .Where(o =>
                        o.PaidAt.HasValue &&
                        o.PaidAt.Value.Date == today
                    )
                    .ToList();


            // =========================================
            // DOANH THU HÔM NAY
            // =========================================

            ViewBag.TodayRevenue =
                todayOrders.Sum(o =>
                    o.TotalAmount
                );


            // =========================================
            // SỐ HÓA ĐƠN HÔM NAY
            // =========================================

            ViewBag.TodayInvoiceCount =
                todayOrders.Count;


            // =========================================
            // SỐ ĐƠN MANG ĐI HÔM NAY
            // =========================================

            ViewBag.TodayTakeAwayCount =
                todayOrders.Count(o =>
                    o.OrderType == "TakeAway"
                );


            // =========================================
            // SỐ BÀN ĐANG PHỤC VỤ
            // =========================================

            ViewBag.ServingTableCount =
                await _context.RestaurantTables
                    .CountAsync(t =>
                        t.Status == "Serving"
                    );


            // =========================================
            // DOANH THU 7 NGÀY GẦN NHẤT
            // =========================================

            var revenueLast7Days =
                new List<DailyStatisticsItem>();


            for (int i = 0; i < 7; i++)
            {
                var date =
                    sevenDaysAgo.AddDays(i);


                var ordersOfDay =
                    paidOrders
                        .Where(o =>
                            o.PaidAt.HasValue &&
                            o.PaidAt.Value.Date == date
                        )
                        .ToList();


                revenueLast7Days.Add(
                    new DailyStatisticsItem
                    {
                        Date =
                            date,

                        DateLabel =
                            date.ToString("dd/MM"),

                        InvoiceCount =
                            ordersOfDay.Count,

                        ItemsSold =
                            ordersOfDay
                                .SelectMany(o =>
                                    o.OrderDetails
                                )
                                .Sum(od =>
                                    od.Quantity
                                ),

                        Revenue =
                            ordersOfDay.Sum(o =>
                                o.TotalAmount
                            )
                    }
                );
            }


            ViewBag.RevenueLast7Days =
                revenueLast7Days;


            // =========================================
            // TOP 5 MÓN BÁN CHẠY TRONG 7 NGÀY
            // =========================================

            var ordersLast7Days =
                paidOrders
                    .Where(o =>
                        o.PaidAt.HasValue &&
                        o.PaidAt.Value.Date >= sevenDaysAgo &&
                        o.PaidAt.Value.Date <= today
                    )
                    .ToList();


            var topFoods =
                ordersLast7Days

                    .SelectMany(o =>
                        o.OrderDetails
                    )

                    .Where(od =>
                        od.Food != null
                    )

                    .GroupBy(od =>
                        new
                        {
                            od.FoodId,

                            Name =
                                od.Food!.Name,

                            ImageUrl =
                                od.Food.ImageUrl
                        }
                    )

                    .Select(group =>
                        new TopFoodItem
                        {
                            FoodId =
                                group.Key.FoodId,

                            FoodName =
                                group.Key.Name,

                            ImageUrl =
                                group.Key.ImageUrl,

                            Quantity =
                                group.Sum(x =>
                                    x.Quantity
                                ),

                            Revenue =
                                group.Sum(x =>
                                    x.Quantity *
                                    x.UnitPrice
                                )
                        }
                    )

                    .OrderByDescending(x =>
                        x.Quantity
                    )

                    .ThenByDescending(x =>
                        x.Revenue
                    )

                    .Take(5)

                    .ToList();


            ViewBag.TopFoods =
                topFoods;


            return View();
        }


        // =========================================
        // TRANG PRIVACY MẶC ĐỊNH
        // =========================================
        public IActionResult Privacy()
        {
            return View();
        }


        // =========================================
        // TRANG LỖI
        // =========================================
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId =
                        Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier
                }
            );
        }
    }
}