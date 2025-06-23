using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Infrastructure.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IMemoryCache _cache;
    private readonly IActionContextAccessor _actionContextAccessor;

    public IdempotencyService(IMemoryCache cache, IActionContextAccessor actionContextAccessor)
    {
        _cache = cache;
        _actionContextAccessor = actionContextAccessor;
    }

    public Task<bool> IsRequestProcessedAsync(string idempotencyKey)
    {
        return Task.FromResult(_cache.TryGetValue(idempotencyKey, out _));
    }

    public Task<IActionResult?> GetCachedResponseAsync(string idempotencyKey)
    {
        if (_cache.TryGetValue(idempotencyKey, out var cachedResponse))
        {
            return Task.FromResult(cachedResponse as IActionResult);
        }
        return Task.FromResult<IActionResult?>(null);
    }

    public Task CacheResponseAsync(string idempotencyKey, IActionResult response)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromDays(1)); // Or a more suitable TTL

        _cache.Set(idempotencyKey, response, cacheEntryOptions);

        return Task.CompletedTask;
    }
} 