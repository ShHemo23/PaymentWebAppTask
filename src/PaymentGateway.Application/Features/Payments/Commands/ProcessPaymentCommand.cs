using MediatR;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Application.Features.Payments.Commands;

public class ProcessPaymentCommand : IRequest<PaymentResponse>
{
    public string CardNumber { get; set; } = null!;
    public int ExpiryMonth { get; set; }
    public int ExpiryYear { get; set; }
    public string Cvv { get; set; } = null!;
    public decimal Amount { get; set; }
}

public sealed record PaymentResponse(string TransactionId, string? RefundCode);
