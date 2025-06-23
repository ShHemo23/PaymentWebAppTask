namespace PaymentGateway.Application.Features.Payments.Queries;
 
public sealed record PaymentDetailsDto(
    string TransactionId,
    decimal Amount,
    string Status,
    DateTimeOffset CreatedDate); 