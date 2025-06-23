using PaymentGateway.Application.Interfaces;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace PaymentGateway.Api.Middleware;

public class IdempotencyMiddleware : IAsyncActionFilter
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(IIdempotencyService idempotencyService, ILogger<IdempotencyMiddleware> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            await next();
            return;
        }

        var key = idempotencyKey.ToString();

        if (await _idempotencyService.IsRequestProcessedAsync(key))
        {
            context.Result = await _idempotencyService.GetCachedResponseAsync(key);
            return;
        }

        var executedContext = await next();

        if (executedContext.Result is not null)
        {
            await _idempotencyService.CacheResponseAsync(key, executedContext.Result);
        }
    }
} 