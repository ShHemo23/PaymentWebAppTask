using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Reports.Queries;

public sealed record GetPaymentsQuery(
    string CardNumber) : IRequest<IEnumerable<Transaction>>;
