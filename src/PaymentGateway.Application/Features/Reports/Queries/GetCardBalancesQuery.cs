using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Reports.Queries;

public sealed record GetCardBalancesQuery(
    string CardNumber) : IRequest<Card>;
