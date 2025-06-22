using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Interfaces;

public interface ICardRepository
{
    Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string cardNumber, CancellationToken cancellationToken = default);
}
