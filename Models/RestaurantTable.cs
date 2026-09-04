namespace banhmihanhphuc.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }

        public string TableName { get; set; } = string.Empty;

        public int Capacity { get; set; } = 4;

        public string Status { get; set; } = "Empty";

        // Một bàn có thể có nhiều hóa đơn theo thời gian
        public List<Order> Orders { get; set; } = new();
    }
}