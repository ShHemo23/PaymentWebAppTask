using MediatR;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Features.Reports.Queries;

public sealed record GetPaymentsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? From = null,
    DateTime? To = null,
    TransactionStatus? Status = null) : IRequest<IEnumerable<PaymentDto>>;

public sealed record PaymentDto(string TransactionId, decimal Amount, string Status, DateTimeOffset CreatedDate);
