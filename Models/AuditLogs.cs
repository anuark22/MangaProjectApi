using System;

namespace Models
{
    public class AuditLogDTO
    {
        public int AuditId { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserRole { get; set; }
        public int? ScanId { get; set; }
        public string? ScanName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? EntityName { get; set; }
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
