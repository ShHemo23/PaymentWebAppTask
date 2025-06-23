using FluentValidation;

namespace PaymentGateway.Application.Features.Auth.Commands;

internal sealed class GetTokenCommandValidator : AbstractValidator<GetTokenCommand>
{
    public GetTokenCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(100);
    }
} 