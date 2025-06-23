using MediatR;
using PaymentGateway.Application.Common;

namespace PaymentGateway.Application.Features.Payments.Queries;

public sealed record GetPaymentQuery(string TransactionId) : IRequest<Result<PaymentDetailsDto>>; 