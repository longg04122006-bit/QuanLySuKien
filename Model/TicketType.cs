namespace Model
{
    public class TicketType
    {
        public int TicketTypeId { get; set; }

        public int EventId { get; set; }

        public string? TicketName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int SoldQuantity { get; set; }

        public DateTime? SaleStart { get; set; }

        public DateTime? SaleEnd { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}