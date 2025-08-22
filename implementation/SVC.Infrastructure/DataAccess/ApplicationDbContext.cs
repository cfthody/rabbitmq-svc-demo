using Bogus;
using Microsoft.EntityFrameworkCore;
using SVC.Core.Entities;
using SVC.Infrastructure.DataAccess.Configurations;

namespace SVC.Infrastructure.DataAccess;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public virtual DbSet<Employee> Employees => Set<Employee>();
    public virtual DbSet<Role> Roles => Set<Role>();
    public virtual DbSet<Platoon> Platoons => Set<Platoon>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("SVC");
        }

        optionsBuilder.UseSeeding((context, _) =>
        {
            if (!context.Set<Role>().Any())
            {
                context
                    .Set<Role>()
                    .AddRange(
                        new Faker<Role>()
                            .RuleFor(r => r.Id, f => f.Random.Long(min: 1, max: 3))
                            .RuleFor(r => r.Name, f => f.Name.JobType())
                            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
                            .Generate(3)
                    );
            }


            if (!context.Set<Platoon>().Any())
            {
                context
                    .Set<Platoon>()
                    .AddRange(
                        new Faker<Platoon>()
                            .RuleFor(r => r.Id, f => f.Random.Long(min: 1, max: 5))
                            .RuleFor(r => r.Name, f => f.Company.CompanyName())
                            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
                            .Generate(4)
                    );
            }

            context.SaveChanges();
        });

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("SVC");

        modelBuilder.ApplyConfiguration(new RoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PlatoonEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}