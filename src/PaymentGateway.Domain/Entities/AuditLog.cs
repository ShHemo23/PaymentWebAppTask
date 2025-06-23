namespace PaymentGateway.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? UserId { get; private set; }
    public string ActionType { get; private set; }
    public string? EntityId { get; private set; }
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string Status { get; private set; }
    public string? IpAddress { get; private set; }

    private AuditLog() 
    {
        ActionType = string.Empty;
        Status = string.Empty;
     } // For EF Core

    public AuditLog(string? userId, string actionType, string status, string? entityId = null, string? oldValue = null, string? newValue = null, string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
        UserId = userId;
        ActionType = actionType;
        Status = status;
        EntityId = entityId;
        OldValue = oldValue;
        NewValue = newValue;
        IpAddress = ipAddress;
    }
} 