using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using banhmihanhphuc.Data;
using banhmihanhphuc.Models;

namespace banhmihanhphuc.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StatisticsController : Controller
    {
        private readonly AppDbContext _context;

        // Kết nối cơ sở dữ liệu
        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // TRANG THỐNG KÊ CHI TIẾT
        // =====================================================
        public async Task<IActionResult> Index(
            string? range,
            DateTime? fromDate,
            DateTime? toDate)
        {
            // =========================================
            // XÁC ĐỊNH KHOẢNG THỜI GIAN
            // =========================================

            DateTime today =
                DateTime.Today;

            DateTime startDate;

            DateTime endDate;


            // Nếu chưa chọn gì thì mặc định 7 ngày gần nhất
            range ??= "7days";


            switch (range)
            {
                // Hôm nay
                case "today":

                    startDate =
                        today;

                    endDate =
                        today;

                    break;


                // 30 ngày gần nhất
                case "30days":

                    startDate =
                        today.AddDays(-29);

                    endDate =
                        today;

                    break;


                // Tùy chọn ngày
                case "custom":

                    startDate =
                        fromDate?.Date
                        ?? today;

                    endDate =
                        toDate?.Date
                        ?? today;

                    break;


                // 7 ngày gần nhất
                default:

                    range =
                        "7days";

                    startDate =
                        today.AddDays(-6);

                    endDate =
                        today;

                    break;
            }


            // Nếu người dùng chọn ngược ngày
            // thì tự đổi lại cho đúng
            if (startDate > endDate)
            {
                var temp =
                    startDate;

                startDate =
                    endDate;

                endDate =
                    temp;
            }


            // =========================================
            // LẤY HÓA ĐƠN ĐÃ THANH TOÁN
            // =========================================
            //
            // Lấy về bộ nhớ trước rồi mới lọc ngày.
            // Cách này tránh lỗi DateTime / timezone
            // giữa C# và PostgreSQL.
            // =========================================

            var allPaidOrders =
                await _context.Orders

                    .Include(o => o.Payment)

                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Food)

                    .Where(o =>
                        o.Status == "Paid" &&
                        o.PaidAt.HasValue
                    )

                    .ToListAsync();


            // =========================================
            // LỌC THEO KHOẢNG NGÀY ĐÃ CHỌN
            // =========================================

            var orders =
                allPaidOrders

                    .Where(o =>
                        o.PaidAt.HasValue &&
                        o.PaidAt.Value.Date >= startDate &&
                        o.PaidAt.Value.Date <= endDate
                    )

                    .ToList();


            // =========================================
            // TẠO MODEL
            // =========================================

            var model =
                new StatisticsViewModel
                {
                    FromDate =
                        startDate,

                    ToDate =
                        endDate
                };


            // =====================================================
            // 1. TỔNG DOANH THU
            // =====================================================

            model.TotalRevenue =
                orders.Sum(o =>
                    o.TotalAmount
                );


            // =====================================================
            // 2. TỔNG SỐ HÓA ĐƠN
            // =====================================================

            model.TotalInvoices =
                orders.Count;


            // =====================================================
            // 3. TỔNG SỐ MÓN ĐÃ BÁN
            // =====================================================

            model.TotalItemsSold =
                orders
                    .SelectMany(o =>
                        o.OrderDetails
                    )
                    .Sum(od =>
                        od.Quantity
                    );


            // =====================================================
            // 4. GIÁ TRỊ TRUNG BÌNH MỖI HÓA ĐƠN
            // =====================================================

            if (model.TotalInvoices > 0)
            {
                model.AverageOrderValue =
                    model.TotalRevenue /
                    model.TotalInvoices;
            }
            else
            {
                model.AverageOrderValue =
                    0;
            }


            // =====================================================
            // 5. ĂN TẠI QUÁN
            // =====================================================

            var dineInOrders =
                orders
                    .Where(o =>
                        o.OrderType == "DineIn"
                    )
                    .ToList();


            model.DineInCount =
                dineInOrders.Count;


            model.DineInRevenue =
                dineInOrders.Sum(o =>
                    o.TotalAmount
                );


            // =====================================================
            // 6. MANG ĐI
            // =====================================================

            var takeAwayOrders =
                orders
                    .Where(o =>
                        o.OrderType == "TakeAway"
                    )
                    .ToList();


            model.TakeAwayCount =
                takeAwayOrders.Count;


            model.TakeAwayRevenue =
                takeAwayOrders.Sum(o =>
                    o.TotalAmount
                );


            // =====================================================
            // 7. THANH TOÁN TIỀN MẶT
            // =====================================================

            var cashOrders =
                orders
                    .Where(o =>
                        o.Payment != null &&
                        o.Payment.PaymentMethod == "Cash"
                    )
                    .ToList();


            model.CashCount =
                cashOrders.Count;


            model.CashRevenue =
                cashOrders.Sum(o =>
                    o.TotalAmount
                );


            // =====================================================
            // 8. THANH TOÁN CHUYỂN KHOẢN
            // =====================================================

            var bankOrders =
                orders
                    .Where(o =>
                        o.Payment != null &&
                        o.Payment.PaymentMethod ==
                            "BankTransfer"
                    )
                    .ToList();


            model.BankTransferCount =
                bankOrders.Count;


            model.BankTransferRevenue =
                bankOrders.Sum(o =>
                    o.TotalAmount
                );


            // =====================================================
            // 9. THỐNG KÊ DOANH THU THEO TỪNG NGÀY
            // =====================================================

            var currentDate =
                startDate;


            while (currentDate <= endDate)
            {
                var date =
                    currentDate;


                var ordersOfDay =
                    orders
                        .Where(o =>
                            o.PaidAt.HasValue &&
                            o.PaidAt.Value.Date == date
                        )
                        .ToList();


                var itemsSold =
                    ordersOfDay
                        .SelectMany(o =>
                            o.OrderDetails
                        )
                        .Sum(od =>
                            od.Quantity
                        );


                var revenue =
                    ordersOfDay
                        .Sum(o =>
                            o.TotalAmount
                        );


                model.DailyStatistics.Add(
                    new DailyStatisticsItem
                    {
                        Date =
                            date,

                        DateLabel =
                            date.ToString("dd/MM"),

                        InvoiceCount =
                            ordersOfDay.Count,

                        ItemsSold =
                            itemsSold,

                        Revenue =
                            revenue
                    }
                );


                currentDate =
                    currentDate.AddDays(1);
            }


            // =====================================================
            // 10. TOP 5 MÓN BÁN CHẠY
            // =====================================================

            var allDetails =
                orders
                    .SelectMany(o =>
                        o.OrderDetails
                    )
                    .Where(od =>
                        od.Food != null
                    )
                    .ToList();


            model.TopFoods =
                allDetails

                    .GroupBy(od =>
                        new
                        {
                            od.FoodId,

                            FoodName =
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
                                group.Key.FoodName,

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


            // =========================================
            // GỬI KIỂU LỌC SANG VIEW
            // =========================================

            ViewBag.Range =
                range;


            return View(model);
        }
    }
}