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
        var requestUrl = "/live"; // Use a public endpoint that doesn't require auth

        // Act
        // Send 5 requests, which should be allowed
        for (int i = 0; i < 5; i++)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUrl));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Send the 6th request, which should be rate limited
        var finalResponse = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUrl));

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, finalResponse.StatusCode);
    }
} 