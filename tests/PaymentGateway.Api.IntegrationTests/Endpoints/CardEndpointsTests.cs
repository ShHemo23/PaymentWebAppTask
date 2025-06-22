using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using PaymentGateway.Application.Features.Cards.Commands;

namespace PaymentGateway.Api.IntegrationTests.Endpoints;

public class CardEndpointsTests : IClassFixture<PaymentGatewayApiFactory>
{
    private readonly HttpClient _client;

    public CardEndpointsTests(PaymentGatewayApiFactory factory)
    {
        _client = factory.CreateClient();
        // Use the mock authentication scheme defined in the factory
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task ValidateCard_WithValidSeededCard_ReturnsSuccess()
    {
        // Arrange
        var command = new ValidateCardCommand
        {
            CardHolderName = "John Smith",
            CardNumber = "4242-4242-4242-4242",
            ExpiryMonth = "12",
            ExpiryYear = "2025",
            Cvv = "123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cards/validate", command);
        var content = await response.Content.ReadFromJsonAsync<ValidateCardResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
        content!.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCard_WithNonExistentCard_ReturnsSuccessWithInvalid()
    {
        // Arrange
        var command = new ValidateCardCommand
        {
            CardHolderName = "Jane Doe",
            CardNumber = "1111-2222-3333-4444",
            ExpiryMonth = "06",
            ExpiryYear = "2028",
            Cvv = "456"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/cards/validate", command);
        var content = await response.Content.ReadFromJsonAsync<ValidateCardResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
        content!.IsValid.Should().BeFalse();
    }
} 