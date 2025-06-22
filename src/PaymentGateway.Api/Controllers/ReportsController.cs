using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Features.Reports.Queries;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] GetPaymentsQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }

    [HttpGet("card-balances")]
    public async Task<IActionResult> GetCardBalances([FromQuery] GetCardBalancesQuery query)
    {
        var result = await _sender.Send(query);
        return Ok(result);
    }
} 