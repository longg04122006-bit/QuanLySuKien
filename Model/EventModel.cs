namespace Model
{
    public class EventModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = "";

        public int CategoryId { get; set; }

        public int VenueId { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int OrganizerId { get; set; }

        public string Status { get; set; } = "";

        public string? ImageUrl { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}