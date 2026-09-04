namespace banhmihanhphuc.Models
{
    // =====================================================
    // MODEL CHÍNH CỦA TRANG THỐNG KÊ
    // =====================================================
    public class StatisticsViewModel
    {
        // Khoảng thời gian đang thống kê
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }


        // =================================================
        // CÁC CHỈ SỐ TỔNG
        // =================================================

        // Tổng doanh thu
        public decimal TotalRevenue { get; set; }

        // Tổng số hóa đơn
        public int TotalInvoices { get; set; }

        // Tổng số món đã bán
        public int TotalItemsSold { get; set; }

        // Giá trị trung bình của một hóa đơn
        public decimal AverageOrderValue { get; set; }


        // =================================================
        // HÌNH THỨC BÁN
        // =================================================

        public int DineInCount { get; set; }

        public int TakeAwayCount { get; set; }

        public decimal DineInRevenue { get; set; }

        public decimal TakeAwayRevenue { get; set; }


        // =================================================
        // PHƯƠNG THỨC THANH TOÁN
        // =================================================

        public int CashCount { get; set; }

        public int BankTransferCount { get; set; }

        public decimal CashRevenue { get; set; }

        public decimal BankTransferRevenue { get; set; }


        // =================================================
        // DOANH THU THEO NGÀY
        // =================================================

        public List<DailyStatisticsItem> DailyStatistics
        {
            get;
            set;
        } = new();


        // =================================================
        // TOP MÓN BÁN CHẠY
        // =================================================

        public List<TopFoodItem> TopFoods
        {
            get;
            set;
        } = new();
    }


    // =====================================================
    // THỐNG KÊ TỪNG NGÀY
    // =====================================================
    public class DailyStatisticsItem
    {
        public DateTime Date { get; set; }

        public string DateLabel { get; set; }
            = string.Empty;

        // Số hóa đơn trong ngày
        public int InvoiceCount { get; set; }

        // Số món đã bán
        public int ItemsSold { get; set; }

        // Doanh thu
        public decimal Revenue { get; set; }
    }


    // =====================================================
    // MÓN BÁN CHẠY
    // =====================================================
    public class TopFoodItem
    {
        public int FoodId { get; set; }

        public string FoodName { get; set; }
            = string.Empty;

        // Tổng số lượng bán
        public int Quantity { get; set; }

        // Doanh thu món đó mang lại
        public decimal Revenue { get; set; }

        // Ảnh món
        public string? ImageUrl { get; set; }
    }
}