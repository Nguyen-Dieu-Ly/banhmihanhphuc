namespace banhmihanhphuc.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string OrderCode { get; set; } = string.Empty;

        public int UserId { get; set; }

        // Có thể null vì đơn mang đi không cần bàn
        public int? TableId { get; set; }

        // DineIn = ăn tại quán
        // TakeAway = mang đi
        public string OrderType { get; set; } = string.Empty;

        // Open, Paid, Cancelled
        public string Status { get; set; } = "Open";

        public decimal Subtotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        // Tài khoản tạo hóa đơn
        public User? User { get; set; }

        // Bàn của hóa đơn
        public RestaurantTable? Table { get; set; }

        // Danh sách món trong hóa đơn
        public List<OrderDetail> OrderDetails { get; set; } = new();

        // Thông tin thanh toán
        public Payment? Payment { get; set; }
    }
}