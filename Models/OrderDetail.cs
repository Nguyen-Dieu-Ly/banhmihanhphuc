namespace banhmihanhphuc.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int FoodId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        // Chi tiết này thuộc hóa đơn nào
        public Order? Order { get; set; }

        // Chi tiết này là món nào
        public Food? Food { get; set; }
    }
}