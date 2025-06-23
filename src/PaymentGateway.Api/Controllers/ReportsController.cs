using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Features.Reports.Queries;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class ReportsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments([FromQuery] GetPaymentsQuery query)
    {
        var result = await _sender.Send(query);

        var baseUrl = Url.Action(nameof(GetPayments), new { version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" });

        var queryDict = HttpContext.Request.Query.ToDictionary(p => p.Key, p => (string?)p.Value.ToString());

        var links = new Dictionary<string, object?>
        {
            ["self"] = new { href = baseUrl + QueryString.Create(queryDict) }
        };

        if (result.Any() && result.Count() == query.PageSize)
        {
            var nextDict = new Dictionary<string,string?>(queryDict) { ["pageNumber"] = (query.PageNumber + 1).ToString() };
            links["next"] = new { href = baseUrl + QueryString.Create(nextDict) };
        }
        if (query.PageNumber > 1)
        {
            var prevDict = new Dictionary<string,string?>(queryDict) { ["pageNumber"] = (query.PageNumber - 1).ToString() };
            links["prev"] = new { href = baseUrl + QueryString.Create(prevDict) };
        }

        return Ok(new { items = result, _links = links });
    }

    [HttpGet("card-balances")]
    public async Task<IActionResult> GetCardBalances([FromQuery] GetCardBalancesQuery query)
    {
        var result = await _sender.Send(query);

        var baseUrl = Url.Action(nameof(GetCardBalances), new { version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1" });
        var queryDict = HttpContext.Request.Query.ToDictionary(p => p.Key, p => (string?)p.Value.ToString());

        var links = new Dictionary<string, object?>
        {
            ["self"] = new { href = baseUrl + QueryString.Create(queryDict) }
        };

        if (result.Any() && result.Count() == query.PageSize)
        {
            var nextDict = new Dictionary<string,string?>(queryDict) { ["pageNumber"] = (query.PageNumber + 1).ToString() };
            links["next"] = new { href = baseUrl + QueryString.Create(nextDict) };
        }
        if (query.PageNumber > 1)
        {
            var prevDict = new Dictionary<string,string?>(queryDict) { ["pageNumber"] = (query.PageNumber - 1).ToString() };
            links["prev"] = new { href = baseUrl + QueryString.Create(prevDict) };
        }

        return Ok(new { items = result, _links = links });
    }
} 