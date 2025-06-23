using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Infrastructure.Services;

namespace PaymentGateway.Infrastructure.Extensions;

public static class CryptoServiceRegistration
{
    public static IServiceCollection AddCrypto(this IServiceCollection services)
    {
        services.AddSingleton<ICryptoService, AesGcmCryptoService>();
        return services;
    }
} 