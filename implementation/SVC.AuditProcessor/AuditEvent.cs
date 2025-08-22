namespace SVC.AuditProcessor;

public class AuditEvent
{
    public long Id { get; set; }
    public string Data { get; set; }
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}