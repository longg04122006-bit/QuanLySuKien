namespace Model
{
    public class Ticket
    {
        public int TicketId { get; set; }

        public string? TicketCode { get; set; }

        public int BookingDetailId { get; set; }

        public int EventId { get; set; }

        public int TicketTypeId { get; set; }

        public string? AttendeeName { get; set; }

        public string? AttendeeEmail { get; set; }

        public string? QRCode { get; set; }

        public string? Status { get; set; }

        public bool CheckedIn { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}