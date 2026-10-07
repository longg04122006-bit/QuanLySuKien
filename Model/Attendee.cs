namespace Model
{
    public class Attendee
    {
        public int AttendeeId { get; set; }

        public int TicketId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}