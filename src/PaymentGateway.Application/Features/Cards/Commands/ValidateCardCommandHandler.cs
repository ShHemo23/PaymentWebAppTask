using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;

namespace PaymentGateway.Application.Features.Cards.Commands;

public class ValidateCardCommandHandler : IRequestHandler<ValidateCardCommand, ValidateCardResponse>
{
    private readonly ICardRepository _cardRepository;
    private readonly ILogger<ValidateCardCommandHandler> _logger;

    public ValidateCardCommandHandler(
        ICardRepository cardRepository,
        ILogger<ValidateCardCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(cardRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _cardRepository = cardRepository;
        _logger = logger;
    }

    public async Task<ValidateCardResponse> Handle(ValidateCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existingCard = await _cardRepository.GetByCardNumberAsync(request.CardNumber, cancellationToken);
            
            if (existingCard == null)
            {
                _logger.LogInformation("Card validation failed: Card not found. CardNumber: {CardNumber}", request.CardNumber);
                return new ValidateCardResponse(false, "Card not found");
            }

            if (existingCard.CardHolderName != request.CardHolderName)
            {
                _logger.LogWarning("Card validation failed: Invalid cardholder name. CardNumber: {CardNumber}", request.CardNumber);
                return new ValidateCardResponse(false, "Invalid cardholder name");
            }

            if (existingCard.ExpiryMonth != request.ExpiryMonth || existingCard.ExpiryYear != request.ExpiryYear)
            {
                _logger.LogWarning("Card validation failed: Invalid expiry date. CardNumber: {CardNumber}", request.CardNumber);
                return new ValidateCardResponse(false, "Invalid expiry date");
            }

            if (existingCard.Cvv != request.Cvv)
            {
                _logger.LogWarning("Card validation failed: Invalid CVV. CardNumber: {CardNumber}", request.CardNumber);
                return new ValidateCardResponse(false, "Invalid CVV");
            }

            _logger.LogInformation("Card validation successful. CardNumber: {CardNumber}", request.CardNumber);
            return new ValidateCardResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating card. CardNumber: {CardNumber}", request.CardNumber);
            throw;
        }
    }
} 