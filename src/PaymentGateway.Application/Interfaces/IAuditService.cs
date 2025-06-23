using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Interfaces;
 
public interface IAuditService
{
    Task LogAsync(AuditLog log);
} 