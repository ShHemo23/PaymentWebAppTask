using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using PaymentGateway.Api.IntegrationTests;
using Xunit;

namespace PaymentGateway.Api.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class HealthCheckEndpointsTests
{
    private readonly HttpClient _client;

    public HealthCheckEndpointsTests(PaymentGatewayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LiveEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ReadyEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
} 