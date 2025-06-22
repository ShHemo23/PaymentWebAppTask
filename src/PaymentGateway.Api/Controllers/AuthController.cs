using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Application.Features.Auth.Commands;

namespace PaymentGateway.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender ?? throw new ArgumentNullException(nameof(sender));

    [HttpPost("token")]
    public async Task<IActionResult> GetToken(GetTokenCommand command)
    {
        var result = await _sender.Send(command);
        return Ok(result);
    }
} 