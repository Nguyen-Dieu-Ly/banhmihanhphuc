namespace banhmihanhphuc.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        // Cash hoặc BankTransfer
        public string PaymentMethod { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.Now;

        // Thanh toán thuộc hóa đơn nào
        public Order? Order { get; set; }
    }
}
