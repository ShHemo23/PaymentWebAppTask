using MediatR;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Features.Cards.Commands;

public class ValidateCardCommandHandler : IRequestHandler<ValidateCardCommand, ValidateCardResponse>
{
    private readonly ILogger<ValidateCardCommandHandler> _logger;

    public ValidateCardCommandHandler(
        ILogger<ValidateCardCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public Task<ValidateCardResponse> Handle(ValidateCardCommand request, CancellationToken cancellationToken)
    {
        var maskedCard = MaskCardNumber(request.CardNumber);

        try
        {
            // 1. Luhn check
            var digitsOnly = string.Concat(request.CardNumber.Where(char.IsDigit));
            if (digitsOnly.Length is < 13 or > 19)
            {
                return Task.FromResult(new ValidateCardResponse(false, "Card number length invalid"));
            }

            bool luhnValid = IsValidLuhn(digitsOnly);
            if (!luhnValid)
            {
                _logger.LogWarning("Card validation failed: Luhn check failed. CardNumber: {CardNumber}", maskedCard);
                return Task.FromResult(new ValidateCardResponse(false, "Invalid card number"));
            }

            // 2. Expiry date must be in the future (end of month)
            var month = request.ExpiryMonth;
            var year = request.ExpiryYear;
            var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            if (expiryDate < DateTime.UtcNow)
            {
                _logger.LogWarning("Card validation failed: Expired card. CardNumber: {CardNumber}", maskedCard);
                return Task.FromResult(new ValidateCardResponse(false, "Card has expired"));
            }

            // 3. CVV 3-4 digits already validated via FluentValidation, but we double-check length.
            if (request.Cvv.Length is < 3 or > 4)
            {
                _logger.LogWarning("Card validation failed: Invalid CVV length. CardNumber: {CardNumber}", maskedCard);
                return Task.FromResult(new ValidateCardResponse(false, "Invalid CVV"));
            }
            
            // All checks passed.
            _logger.LogInformation("Card validation successful. CardNumber: {CardNumber}", maskedCard);
            return Task.FromResult(new ValidateCardResponse(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating card. CardNumber: {CardNumber}", maskedCard);
            throw;
        }
    }

    private static bool IsValidLuhn(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(cardNumber[i])) return false;
            int n = cardNumber[i] - '0';
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    private static string MaskCardNumber(string cardNumber)
    {
        var digits = string.Concat(cardNumber.Where(char.IsDigit));
        if (digits.Length <= 4)
        {
            return new string('*', digits.Length);
        }

        var last4 = digits[^4..];
        return new string('*', digits.Length - 4) + last4;
    }
} 