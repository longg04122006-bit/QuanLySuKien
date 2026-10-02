namespace Model
{
    public class CheckIn
    {
        public int CheckInId { get; set; }
        public int TicketId { get; set; }
        public DateTime CheckInTime { get; set; }
        public int CheckedInBy { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}