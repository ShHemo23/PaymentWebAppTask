using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;

    public AuditService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(AuditLog log)
    {
        await _context.AuditLogs.AddAsync(log);
        // This SaveChanges is separate to ensure audit logs are committed
        // even if the main transaction is rolled back.
        await _context.SaveChangesAsync(default);
    }
} 