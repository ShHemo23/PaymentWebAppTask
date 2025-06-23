using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Application.Common;

namespace PaymentGateway.Application.Features.Payments.Queries;

internal sealed class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Result<PaymentDetailsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentQueryHandler(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Result<PaymentDetailsDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
    {
        var txn = await _context.Transactions.AsNoTracking()
            .FirstOrDefaultAsync(t => t.PublicTransactionId == request.TransactionId, cancellationToken);

        if (txn == null)
        {
            return Result<PaymentDetailsDto>.Failure(Error.NotFound());
        }

        var dto = new PaymentDetailsDto(
            txn.PublicTransactionId,
            txn.Amount,
            txn.Status.ToString(),
            txn.CreatedDate);

        return Result<PaymentDetailsDto>.Success(dto);
    }
} 