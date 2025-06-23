## Error-handling Migration – `Result<T>` Pattern

The codebase is being refactored to adopt an explicit `Result<T>` / `Error` pattern (see `PaymentGateway.Application.Common`). This section explains how to **write new handlers correctly** and **incrementally migrate** existing ones without breaking runtime behaviour.

### 1 · New code – always return `Result<T>`

* Define the MediatR request to return `Result<Payload>` rather than `Payload`.

  ```csharp
  public record GetFooQuery(Guid Id) : IRequest<Result<FooDto>>;
  ```

* In the handler use the factory helpers:

  ```csharp
  return Result<FooDto>.Success(dto);
  return Result<FooDto>.Failure(Error.NotFound());
  ```

* Controllers translate the result into the correct HTTP response:

  ```csharp
  var result = await _sender.Send(query);

  if (!result.IsSuccess)
  {
      return result.Error?.Code switch
      {
          "not_found"        => NotFound(result.Error!.Message),
          "validation_error" => BadRequest(result.Error!.Message),
          _                    => StatusCode(409, result.Error!.Message)
      };
  }

  return Ok(result.Value);
  ```

### 2 · Migrating existing handlers incrementally

1. Change the handler interface from `IRequest<Payload>` to `IRequest<Result<Payload>>`.
2. Replace the successful return value with `Result.Success(payload)`.
3. Convert business‐rule exceptions into `Result.Failure(Error.X)` instead of `throw` – **system exceptions should still be thrown**.
4. Update the calling controller / pipeline behaviour to unwrap the `Result`.
5. Adjust unit tests to assert `IsSuccess` / `Error` rather than catching exceptions.

Because the `GlobalExceptionHandler` is still active, legacy handlers that throw continue to map to HTTP 4xx/5xx until they are upgraded, enabling a safe endpoint-by-endpoint migration.

### 3 · After ~100 % coverage

Once almost every handler returns `Result<T>` the **`GlobalExceptionHandler`** can be simplified to a last-resort *"5xx mapper"* that only turns truly unexpected exceptions into 500 responses:

```csharp
app.UseExceptionHandler(cfg =>
{
    cfg.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "Unexpected error" });
    });
});
```

Normal business errors are now surfaced intentionally through the `Result<T>` contract, leaving the middleware to deal exclusively with infrastructure or programming faults. 