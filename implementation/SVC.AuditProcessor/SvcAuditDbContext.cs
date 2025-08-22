using Audit.Core;
using Audit.EntityFramework;
using Audit.PostgreSql;
using Audit.PostgreSql.Configuration;
using Audit.PostgreSql.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SVC.AuditProcessor;

// [AuditDbContext(ReloadDatabaseValues = true)]
public sealed class SvcAuditDbContext : AuditDbContext
{
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public SvcAuditDbContext(DbContextOptions<SvcAuditDbContext> options): base(options)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        
        AuditDisabled = false;
        ReloadDatabaseValues = true;
        AuditDataProvider = new PostgreSqlDataProvider()
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection"),
            Schema = "AE",
            TableName = "AuditEvents",
            IdColumnName = "ID",
            DataColumnName = "Data",
            DataType = "JSONB",
            LastUpdatedDateColumnName = "UpdatedAt",
            CustomColumns =
            [
                new CustomColumn("event_type", ev => ev.EventType),
                new CustomColumn("user", ev => ev.Environment.UserName),
                new CustomColumn("host", ev => ev.Environment.MachineName),
            ]
        };
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("AE");

        modelBuilder.ApplyConfiguration(new AuditEventEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}