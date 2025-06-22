using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace PaymentGateway.Api.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class RateLimitingTests
{
    private readonly HttpClient _client;

    public RateLimitingTests(PaymentGatewayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task AnyEndpoint_WhenCalledRepeatedly_ReturnsTooManyRequests()
    {
        // Arrange
        var requestUrl = "/live"; // Public endpoint without auth
        var clientId = Guid.NewGuid().ToString();

        // Act
        // Send 5 requests, which should be allowed
        for (int i = 0; i < 5; i++)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            msg.Headers.Add("X-ClientId", clientId);
            var response = await _client.SendAsync(msg);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Send the 6th request, which should be rate limited
        var finalMsg = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        finalMsg.Headers.Add("X-ClientId", clientId);
        var finalResponse = await _client.SendAsync(finalMsg);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, finalResponse.StatusCode);
    }
} 