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
using Microsoft.Data.SqlClient;

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

    private readonly string _testDbConnectionString;

    public PaymentGatewayApiFactory()
    {
        // 1) Start the container synchronously
        _dbContainer.StartAsync().GetAwaiter().GetResult();

        // 2) Build a connection string that targets a dedicated test database instead of master
        var connectionBuilder = new SqlConnectionStringBuilder(_dbContainer.GetConnectionString())
        {
            InitialCatalog = "PaymentGatewayTestDb" // this sets 'Database=' in the connection string
        };
        _testDbConnectionString = connectionBuilder.ToString();

        // 3) Inject the connection string as an environment variable so Program.cs picks it up
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            _testDbConnectionString);
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

            // Re-register against our dedicated test database inside the container
            services.AddDbContext<PaymentDbContext>(opts =>
                opts.UseSqlServer(_testDbConnectionString));

            // Swap in test authentication
            services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentGateway.Infrastructure.Data.PaymentDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        if (!dbContext.Cards.Any(c => c.CardNumber == "4242-4242-4242-4242"))
        {
            dbContext.Cards.Add(new(
                "John Smith",
                "4242-4242-4242-4242",
                "12",
                "2025",
                "123",
                1000m));
        }
        else
        {
            var existing = await dbContext.Cards.FirstAsync(c => c.CardNumber == "4242-4242-4242-4242");
            existing.GetType().GetProperty("ExpiryMonth")!.SetValue(existing, "12");
            existing.GetType().GetProperty("ExpiryYear")!.SetValue(existing, "2025");
            existing.GetType().GetProperty("Cvv")!.SetValue(existing, "123");
            existing.GetType().GetProperty("CardHolderName")!.SetValue(existing, "John Smith");
            existing.GetType().GetProperty("Balance")!.SetValue(existing, 1000m);
        }
        await dbContext.SaveChangesAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => Task.CompletedTask;

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
