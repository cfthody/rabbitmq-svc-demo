// See https://aka.ms/new-console-template for more information

using Audit.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SVC.AuditProcessor;
using SVC.SharedKernel;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((host, services) =>
    {
        services.RegisterRabbitMqDependencies();
        
        services.AddDbContext<SvcAuditDbContext>(
            options => options.UseNpgsql(host.Configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IAuditScopeFactory, AuditScopeFactory>();
        
        services.AddHostedService<AuditManagerEventConsumer>();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SvcAuditDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

await host.RunAsync();