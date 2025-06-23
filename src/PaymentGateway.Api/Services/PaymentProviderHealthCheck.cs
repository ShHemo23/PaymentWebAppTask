using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PaymentGateway.Api.Services;

public sealed class PaymentProviderHealthCheck(HttpClient client) : IHealthCheck
{
    private readonly HttpClient _client = client;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // If base address has not been configured we treat the external dependency as *optional* and mark the service healthy.
            if (_client.BaseAddress is null)
            {
                return HealthCheckResult.Healthy("Payment provider base address not configured – assuming healthy in this environment");
            }

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