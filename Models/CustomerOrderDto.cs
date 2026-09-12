namespace banhmihanhphuc.Models
{
    public class CustomerOrderDto
    {
        public int TableId { get; set; }

        public string? Note { get; set; }

        public List<CustomerOrderItemDto> Items { get; set; }
            = new();
    }

    public class CustomerOrderItemDto
    {
        public int FoodId { get; set; }

        public int Quantity { get; set; }
    }
}