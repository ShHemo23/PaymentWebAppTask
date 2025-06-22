using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Features.Payments.Commands;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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

    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment(RefundPaymentCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
} 