using Microsoft.Extensions.DependencyInjection;

namespace SVC.SharedKernel;

public static class DependencyInjections
{
    public static void RegisterRabbitMqDependencies( this IServiceCollection services)
    {
        services.AddScoped<IRabbitMqConnection, RabbitMqConnection>();
    }
}