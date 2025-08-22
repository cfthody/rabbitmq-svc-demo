using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SVC.Infrastructure.DataAccess;

namespace SVC.Infrastructure;

public static class DependencyInjections
{
    public static void RegisterInfrastructureDependencies(
        this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
        );
        
        services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();
    }
}