using FluentValidation;

namespace PaymentGateway.Application.Features.Payments.Commands;

internal sealed class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .Length(10, 20)
            .Matches("^[A-Za-z0-9]+$")
            .WithMessage("TransactionId must be 10-20 alphanumeric characters.");

        RuleFor(x => x.RefundCode)
            .NotEmpty()
            .Matches("^\\d{4}$")
            .WithMessage("RefundCode must be exactly 4 digits.");
    }
} 