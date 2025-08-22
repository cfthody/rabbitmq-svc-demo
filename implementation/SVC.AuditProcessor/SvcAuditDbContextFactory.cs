using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SVC.AuditProcessor;

public class SvcAuditDbContextFactory : IDesignTimeDbContextFactory<SvcAuditDbContext>
{
    public SvcAuditDbContext CreateDbContext()
        => CreateDbContext(Array.Empty<string>());

    public SvcAuditDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<SvcAuditDbContext>();
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .EnableServiceProviderCaching(false)
            .EnableSensitiveDataLogging();

        return new SvcAuditDbContext(optionsBuilder.Options);
    }
}