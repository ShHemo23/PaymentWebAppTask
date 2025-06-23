namespace PaymentGateway.Domain.Enums;

public enum TransactionStatus
{
    Pending,
    Successful,
    Failed,
    Authorized,
    Refunded
}
