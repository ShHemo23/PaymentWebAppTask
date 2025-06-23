using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;
using System.Security.Cryptography;

namespace PaymentGateway.Application.Features.Auth.Commands;

internal sealed class GetTokenCommandHandler : IRequestHandler<GetTokenCommand, string>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GetTokenCommandHandler> _logger;
    private readonly IAuditService _auditService;

    public GetTokenCommandHandler(IConfiguration configuration, ILogger<GetTokenCommandHandler> logger, IAuditService auditService)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(auditService);
        _configuration = configuration;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<string> Handle(GetTokenCommand request, CancellationToken cancellationToken)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = jwtSettings["Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            // When running in test or local environments the key may be absent. Generate a temporary one on the fly.
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            key = Convert.ToBase64String(randomBytes);
            _logger.LogWarning("JWT key not found in configuration. Generated a transient key for this runtime instance – *tokens issued with this key will not be valid across restarts*.");
        }

        var issuer = jwtSettings["Issuer"] ?? "PaymentGateway.Api";
        var audience = jwtSettings["Audience"] ?? "PaymentGateway.Users";

        // Hard-coded credentials from configuration for demo purposes
        var validUsername = _configuration["Auth:Username"] ?? "admin";
        var validPassword = _configuration["Auth:Password"] ?? "P@ssw0rd!";

        if (request.Username != validUsername || request.Password != validPassword)
        {
            _logger.LogWarning("Invalid credentials for user {Username}", request.Username);
            await _auditService.LogAsync(new AuditLog(request.Username, "LoginAttempt", "Failure"));
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var claims = new List<Claim> { new (ClaimTypes.Name, request.Username) };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var serializedToken = tokenHandler.WriteToken(token);

        await _auditService.LogAsync(new AuditLog(request.Username, "LoginAttempt", "Success"));
        return serializedToken;
    }
} 