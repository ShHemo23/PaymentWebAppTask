using MediatR;

namespace PaymentGateway.Application.Features.Reports.Queries;

public sealed record GetCardBalancesQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<IEnumerable<CardBalanceDto>>;

public sealed record CardBalanceDto(string CardHash, decimal Balance);
