using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Card> Cards { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
