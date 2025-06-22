using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Payments.Commands;

public sealed record RefundPaymentCommand(
    Guid TransactionId) : IRequest<Transaction>;
