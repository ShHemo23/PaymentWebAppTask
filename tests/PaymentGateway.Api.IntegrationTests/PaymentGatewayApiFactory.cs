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

namespace PaymentGateway.Api.IntegrationTests;

public class PaymentGatewayApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("localdev!123")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilMessageIsLogged("SQL Server is now ready for client connections."))
        .Build();

    public PaymentGatewayApiFactory()
    {
        // 1) Start the container synchronously
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        // 2) Inject the connection string as an environment variable
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _dbContainer.GetConnectionString());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the original DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<PaymentDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Re-register against our live container
            services.AddDbContext<PaymentDbContext>(opts =>
                opts.UseSqlServer(_dbContainer.GetConnectionString()));

            // Swap in test authentication
            services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    // Seed data after host is built
    public async Task SeedAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var seeded = dbContext.Cards.SingleOrDefault(c => c.CardNumber == "4242-4242-4242-4242");
        if (seeded is null)
        {
            dbContext.Cards.Add(new PaymentGateway.Domain.Entities.Card(
                "John Smith",
                "4242-4242-4242-4242",
                "12",
                "2025",
                "123",
                1000m));
        }
        else
        {
            // ensure details are correct
            seeded.Credit(0); // no-op ensure tracked
        }
        await dbContext.SaveChangesAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => SeedAsync();

    public override async ValueTask DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}

// Mock Authentication Handler to bypass real JWT in tests
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
