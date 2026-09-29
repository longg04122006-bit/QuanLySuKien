namespace Model
{
    public class BookingDetail
    {
        public int BookingDetailId { get; set; }

        public int BookingId { get; set; }

        public int TicketTypeId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Amount { get; set; }
    }
}