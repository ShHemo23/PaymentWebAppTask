using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;

namespace PaymentGateway.Infrastructure.Persistence.Repositories;

public sealed class CardRepository : ICardRepository
{
    private readonly IApplicationDbContext _context;

    public CardRepository(IApplicationDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public async Task<Card?> GetByCardNumberAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardNumber, cancellationToken);
    }

    public async Task AddAsync(Card card, CancellationToken cancellationToken = default)
    {
        await _context.Cards.AddAsync(card, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Cards.AnyAsync(c => c.CardNumber == cardNumber, cancellationToken);
    }
} 