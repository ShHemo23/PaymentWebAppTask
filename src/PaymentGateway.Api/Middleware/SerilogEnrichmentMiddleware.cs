namespace PaymentGateway.Api.Middleware;

using System.Diagnostics;
using Serilog.Context;

public sealed class SerilogEnrichmentMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("TraceId", Activity.Current?.TraceId.ToString()))
        using (LogContext.PushProperty("SpanId", Activity.Current?.SpanId.ToString()))
        {
            var userId = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : null;
            if (!string.IsNullOrWhiteSpace(userId))
            {
                LogContext.PushProperty("UserId", userId);
            }

            if (context.Request.Headers.TryGetValue("X-Transaction-Id", out var txId) && !string.IsNullOrWhiteSpace(txId))
            {
                LogContext.PushProperty("TransactionId", txId.ToString());
            }

            await _next(context);
        }
    }
} 