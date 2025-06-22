using FluentValidation;
using MediatR;

namespace PaymentGateway.Application.Features.Cards.Commands;

public record ValidateCardCommand : IRequest<ValidateCardResponse>
{
    public required string CardHolderName { get; init; }
    public required string CardNumber { get; init; }
    public required string ExpiryMonth { get; init; }
    public required string ExpiryYear { get; init; }
    public required string Cvv { get; init; }
}

public record ValidateCardResponse(bool IsValid, string? Error = null);

public class ValidateCardCommandValidator : AbstractValidator<ValidateCardCommand>
{
    public ValidateCardCommandValidator()
    {
        RuleFor(x => x.CardHolderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .Matches(@"^\d{4}-\d{4}-\d{4}-\d{4}$")
            .WithMessage("Card number must be in format: xxxx-xxxx-xxxx-xxxx");

        RuleFor(x => x.ExpiryMonth)
            .NotEmpty()
            .Matches(@"^(0[1-9]|1[0-2])$")
            .WithMessage("Expiry month must be between 01 and 12");

        RuleFor(x => x.ExpiryYear)
            .NotEmpty()
            .Matches(@"^\d{4}$")
            .WithMessage("Expiry year must be a 4-digit number")
            .Must(year =>
            {
                if (int.TryParse(year, out int yearInt))
                {
                    return yearInt >= DateTime.UtcNow.Year;
                }
                return false;
            })
            .WithMessage("Card has expired");

        RuleFor(x => x.Cvv)
            .NotEmpty()
            .Matches(@"^\d{3,4}$")
            .WithMessage("CVV must be 3 or 4 digits");
    }
}
