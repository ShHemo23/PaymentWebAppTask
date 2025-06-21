using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Common.Behaviors;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Infrastructure.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.Load("PaymentGateway.Application")));
        
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        services.AddValidatorsFromAssembly(Assembly.Load("PaymentGateway.Application"), includeInternalTypes: true);

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<PaymentDbContext>());
        
        services.AddScoped<ICardRepository, PaymentGateway.Infrastructure.Persistence.Repositories.CardRepository>();

        services.AddSingleton<IHostedService, PaymentGateway.Infrastructure.Services.AutomaticConfirmationService>();

        return services;
    }
} 