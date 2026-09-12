namespace banhmihanhphuc.Models
{
    public class CustomerOrderRequestDetail
    {
        public int Id { get; set; }

        public int CustomerOrderRequestId { get; set; }

        public int FoodId { get; set; }

        public int Quantity { get; set; }

        public CustomerOrderRequest? CustomerOrderRequest { get; set; }

        public Food? Food { get; set; }
    }
}