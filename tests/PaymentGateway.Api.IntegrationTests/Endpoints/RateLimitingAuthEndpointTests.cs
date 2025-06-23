using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PaymentGateway.Application.Features.Auth.Commands;

namespace PaymentGateway.Api.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class RateLimitingAuthEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly PaymentGatewayApiFactory _factory;

    public RateLimitingAuthEndpointTests(PaymentGatewayApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Assign headers to simulate real-world client & IP detection
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
        _client.DefaultRequestHeaders.Add("X-Real-IP", "9.9.9.9");
        _client.DefaultRequestHeaders.Add("X-ClientId", "test-auth-client");
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task TokenEndpoint_WhenCalledRepeatedly_ReturnsTooManyRequests()
    {
        // Arrange
        const string requestUrl = "/api/v1/auth/token";
        var command = new GetTokenCommand("admin", "P@ssw0rd!");

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