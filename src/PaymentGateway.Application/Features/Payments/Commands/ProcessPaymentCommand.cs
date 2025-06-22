using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Payments.Commands;

public sealed record ProcessPaymentCommand(
    string CardNumber,
    string Cvv,
    int ExpiryMonth,
    int ExpiryYear,
    decimal Amount,
    string Currency) : IRequest<Transaction>;
