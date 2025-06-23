using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Enums;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace PaymentGateway.Application.Features.Reports.Queries;

internal sealed class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, IEnumerable<PaymentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;

    public GetPaymentsQueryHandler(IApplicationDbContext context, Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(cache);
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<PaymentDto>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"payments:{request.From}:{request.To}:{request.Status}:{request.PageNumber}:{request.PageSize}";

        if (_cache.TryGetValue(cacheKey, out var cachedObj) && cachedObj is IEnumerable<PaymentDto> cached)
        {
            return cached;
        }

        var query = _context.Transactions.AsNoTracking();

        if (request.From.HasValue)
            query = query.Where(t => t.CreatedDate >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(t => t.CreatedDate <= request.To.Value);
        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        query = query.OrderByDescending(t => t.CreatedDate)
                     .Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize);

        var data = await query.Select(t => new PaymentDto(
            t.PublicTransactionId,
            t.Amount,
            t.Status.ToString(),
            t.CreatedDate)).ToListAsync(cancellationToken);

        _cache.Set(cacheKey, data, TimeSpan.FromSeconds(30));

        return data;
    }
} 