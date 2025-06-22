using MediatR;

namespace PaymentGateway.Application.Features.Auth.Commands;

public sealed record GetTokenCommand(
    string CardNumber,
    string Cvv,
    int ExpiryMonth,
    int ExpiryYear) : IRequest<string>;
