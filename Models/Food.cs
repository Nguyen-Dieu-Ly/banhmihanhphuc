namespace banhmihanhphuc.Models
{
    public class Food
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Mỗi món ăn thuộc một danh mục
        public Category? Category { get; set; }

        // Một món có thể xuất hiện trong nhiều chi tiết hóa đơn
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}