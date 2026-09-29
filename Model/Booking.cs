namespace Model
{
    public class Booking
    {
        public int BookingId { get; set; }
        public string? BookingCode { get; set; }
        public int UserId { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}