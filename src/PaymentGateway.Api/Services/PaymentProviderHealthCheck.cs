using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaymentGateway.Api.Services;

public sealed class PaymentProviderHealthCheck(HttpClient client) : IHealthCheck
{
    private readonly HttpClient _client = client;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Payment provider unhealthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Payment provider unreachable", ex);
        }
    }
} 