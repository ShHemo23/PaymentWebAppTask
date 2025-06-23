using FluentValidation;

namespace PaymentGateway.Application.Features.Payments.Commands;

internal sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.CardNumber).NotEmpty().Matches("^[0-9\\-\\s]{13,25}$");
        RuleFor(x => x.ExpiryMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.ExpiryYear).GreaterThanOrEqualTo(DateTime.UtcNow.Year);
        RuleFor(x => x.Cvv).NotEmpty().Matches("^\\d{3,4}$");
        RuleFor(x => x.Amount).GreaterThan(0);
    }
} 