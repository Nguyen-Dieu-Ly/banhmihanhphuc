namespace banhmihanhphuc.Models
{
    public class CustomerOrderRequest
    {
        public int Id { get; set; }

        public int TableId { get; set; }

        // Pending / Accepted / Rejected
        public string Status { get; set; } = "Pending";

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? AcceptedAt { get; set; }

        public DateTime? RejectedAt { get; set; }

        public RestaurantTable? Table { get; set; }

        public List<CustomerOrderRequestDetail> Details { get; set; }
            = new();
    }
}