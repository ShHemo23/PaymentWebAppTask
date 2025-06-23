using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Infrastructure.Services;

namespace PaymentGateway.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrypto(this IServiceCollection services)
    {
        services.AddSingleton<ICryptoService, AesGcmCryptoService>();
        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHostedService<AutomaticConfirmationService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        return services;
    }
} 