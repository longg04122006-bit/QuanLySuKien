namespace Model
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int BookingId { get; set; }

        public string? PaymentCode { get; set; }

        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? Status { get; set; }

        public string? TransactionCode { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}