using FluentValidation;
using MediatR;

namespace PaymentGateway.Application.Features.Cards.Commands;

public record ValidateCardCommand : IRequest<ValidateCardResponse>
{
    public required string CardNumber { get; init; }
    public required int ExpiryMonth { get; init; }
    public required int ExpiryYear { get; init; }
    public required string Cvv { get; init; }
    public string? CardHolderName { get; init; }
}

public record ValidateCardResponse(bool IsValid, string? Message = null);

public class ValidateCardCommandValidator : AbstractValidator<ValidateCardCommand>
{
    public ValidateCardCommandValidator()
    {
        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Matches(@"^[0-9\-\s]{13,25}$")
            .WithMessage("Card number must contain 13-19 digits and may include spaces or dashes");

        RuleFor(x => x.ExpiryMonth)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.ExpiryYear)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .Matches(@"^\d{3,4}$")
            .WithMessage("CVV must be 3 or 4 digits");
    }
}
