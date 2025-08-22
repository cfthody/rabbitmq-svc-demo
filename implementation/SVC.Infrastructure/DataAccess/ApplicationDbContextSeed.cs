using Bogus;
using SVC.Core.Entities;

namespace SVC.Infrastructure.DataAccess;

public class ApplicationDbContextSeed : IDbSeeder<ApplicationDbContext>
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        context
            .Set<Employee>()
            .AddRange(
                new Faker<Employee>()
                    .RuleFor(e => e.Id, f => f.Random.Long(min: 1, max: long.MaxValue))
                    .RuleFor(e => e.Name, f => f.Name.FirstName())
                    .RuleFor(e => e.EntryDate, f => f.Date.Past())
                    .Generate(3)
            );
        
        await context.SaveChangesAsync();
    }
}