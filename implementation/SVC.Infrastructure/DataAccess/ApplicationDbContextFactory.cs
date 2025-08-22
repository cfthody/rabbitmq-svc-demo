using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SVC.Core.Entities;

namespace SVC.Infrastructure.DataAccess;

public interface IApplicationDbContextFactory
{
    ApplicationDbContext CreateDbContext();
}

internal class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>, IApplicationDbContextFactory
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .EnableServiceProviderCaching(false)
            .EnableSensitiveDataLogging();

        return new ApplicationDbContext(optionsBuilder.Options);

    }

    public ApplicationDbContext CreateDbContext()
        => CreateDbContext(Array.Empty<string>());
}