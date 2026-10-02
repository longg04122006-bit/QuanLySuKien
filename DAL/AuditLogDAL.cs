namespace Model
{
    public class AuditLog
    {
        public long AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? TableName { get; set; }
        public int? RecordId { get; set; }
        public string? OldData { get; set; }
        public string? NewData { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}