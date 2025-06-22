using System.Security.Claims;
using System.Text.Encodings.Web;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Infrastructure.Data;
using Testcontainers.MsSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace PaymentGateway.Api.IntegrationTests;

public class PaymentGatewayApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("localdev!123")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("SQL Server is now ready for client connections."))
        .Build();

    public PaymentGatewayApiFactory()
    {
        // Start the container synchronously when the factory is created.
        // This ensures the container is running before any host configuration begins.
        _dbContainer.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inject the test container's connection string into IConfiguration
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbContainer.GetConnectionString()
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove the original DbContext registration
            var descriptor = services.SingleOrDefault(d => 
                d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Re-register against our live container
            services.AddDbContext<PaymentDbContext>(opts =>
                opts.UseSqlServer(_dbContainer.GetConnectionString()));

            // Swap in test auth
            services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    // 1) Override the factory's ValueTask DisposeAsync
    public override async ValueTask DisposeAsync()
    {
        // First let Testcontainers tear down the container
        await _dbContainer.DisposeAsync();

        // Then let the base class clean up
        await base.DisposeAsync();
    }

    // 2) Explicitly implement IAsyncLifetime.DisposeAsync (returns Task)
    async Task IAsyncLifetime.DisposeAsync()
    {
        // Only dispose the container here
        await _dbContainer.DisposeAsync();
    }
}

// Mock Authentication Handler to bypass real JWT validation in tests
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
} 