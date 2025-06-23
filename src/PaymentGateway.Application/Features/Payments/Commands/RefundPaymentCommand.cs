using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Payments.Commands;

public sealed record RefundPaymentCommand(
    string TransactionId,
    string RefundCode) : IRequest<Unit>;
