using MediatR;

namespace PaymentGateway.Application.Features.Auth.Commands;

public sealed record GetTokenCommand(
    string Username,
    string Password) : IRequest<string>;
