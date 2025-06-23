using Microsoft.AspNetCore.Mvc;

namespace PaymentGateway.Application.Interfaces;

public interface IIdempotencyService
{
    Task<bool> IsRequestProcessedAsync(string idempotencyKey);
    Task<IActionResult?> GetCachedResponseAsync(string idempotencyKey);
    Task CacheResponseAsync(string idempotencyKey, IActionResult response);
} 