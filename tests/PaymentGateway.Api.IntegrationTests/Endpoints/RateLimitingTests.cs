using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PaymentGateway.Application.Features.Cards.Commands;

namespace PaymentGateway.Api.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class RateLimitingTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly PaymentGatewayApiFactory _factory;

    public RateLimitingTests(PaymentGatewayApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        _client.DefaultRequestHeaders.Add("X-Real-IP", "1.2.3.4");
        _client.DefaultRequestHeaders.Add("X-ClientId", "test-client");
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ApiEndpoint_WhenCalledRepeatedly_ReturnsTooManyRequests()
    {
        // Arrange
        var requestUrl = "/api/v1/cards/validate";
        var command = new ValidateCardCommand
        {
            CardHolderName = "Jane Doe",
            CardNumber = "4539-6829-9582-4395",
            ExpiryMonth = 6,
            ExpiryYear = 2028,
            Cvv = "456"
        };
        
        var responses = new List<HttpResponseMessage>();
        const int requestCount = 7;
        const int limit = 5;

        // Act
        for (var i = 0; i < requestCount; i++)
        {
            var response = await _client.PostAsJsonAsync(requestUrl, command);
            responses.Add(response);
        }

        // Assert
        responses.Take(limit).Should().OnlyContain(r => r.IsSuccessStatusCode);
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }
} 