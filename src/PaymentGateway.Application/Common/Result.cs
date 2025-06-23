namespace PaymentGateway.Application.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(bool success, T? value, Error? error)
    {
        IsSuccess = success;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(Error error) => new(false, default, error);
}

public sealed record Error(string Code, string Message)
{
    public static Error NotFound(string message = "Resource not found") => new("not_found", message);
    public static Error Validation(string message) => new("validation_error", message);
    public static Error Conflict(string message) => new("conflict", message);
    public static Error InsufficientFunds(string message="Insufficient funds")=> new("insufficient_funds",message);
} 