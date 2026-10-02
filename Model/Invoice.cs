namespace Model
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public string? InvoiceCode { get; set; }

        public int BookingId { get; set; }

        public DateTime InvoiceDate { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerEmail { get; set; }

        public decimal TotalAmount { get; set; }

        public string? PDFUrl { get; set; }
    }
}