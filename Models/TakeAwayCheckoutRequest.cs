namespace banhmihanhphuc.Models
{
    public class TakeAwayCheckoutRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;

        public List<TakeAwayItemRequest> Items { get; set; } = new();
    }

    public class TakeAwayItemRequest
    {
        public int FoodId { get; set; }

        public int Quantity { get; set; }
    }
}