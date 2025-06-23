using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Features.Payments.Commands;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class PaymentsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpPost]
    public async Task<IActionResult> ProcessPayment(ProcessPaymentCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }

    [HttpPost("~/api/refunds")]
    public async Task<IActionResult> RefundPayment(RefundPaymentCommand command)
    {
        await _sender.Send(command);
        return NoContent();
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPaymentById(string id)
    {
        var result = await _sender.Send(new PaymentGateway.Application.Features.Payments.Queries.GetPaymentQuery(id));

        if (!result.IsSuccess || result.Value is null)
        {
            return result.Error?.Code switch
            {
                "not_found" => NotFound(result.Error?.Message),
                _ => BadRequest(result.Error?.Message)
            };
        }

        var details = result.Value;

        var links = new
        {
            self = new { href = Url.Action(nameof(GetPaymentById), new { id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" }) },
            refund = new { href = Url.Action("RefundPayment", "Payments", new { version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" }, Request.Scheme) }
        };

        return Ok(new
        {
            details.TransactionId,
            details.Amount,
            details.Status,
            details.CreatedDate,
            _links = links
        });
    }
} 