using Microsoft.EntityFrameworkCore;

namespace SVC.Infrastructure.DataAccess;

public interface IDbSeeder<in TContext> where TContext : DbContext
{
    Task SeedAsync(TContext context);
}