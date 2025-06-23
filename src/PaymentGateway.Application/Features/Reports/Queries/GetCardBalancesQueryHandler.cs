using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Interfaces;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace PaymentGateway.Application.Features.Reports.Queries;

internal sealed class GetCardBalancesQueryHandler : IRequestHandler<GetCardBalancesQuery, IEnumerable<CardBalanceDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public GetCardBalancesQueryHandler(IApplicationDbContext context, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(cache);
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<CardBalanceDto>> Handle(GetCardBalancesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"card-balances:{request.PageNumber}:{request.PageSize}";

        if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is IEnumerable<CardBalanceDto> cached)
        {
            return cached;
        }

        var data = await _context.Cards.AsNoTracking()
            .OrderBy(c => c.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CardBalanceDto(c.CardNumber, c.Balance))
            .ToListAsync(cancellationToken);

        _cache.Set(cacheKey, data, TimeSpan.FromMinutes(1));

        return data;
    }
} 