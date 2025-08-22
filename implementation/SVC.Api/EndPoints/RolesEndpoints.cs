using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVC.Core.Entities;
using SVC.Infrastructure.DataAccess;
using SVC.SharedKernel;

namespace SVC.Api.EndPoints;

public static class RolesEndpoints
{
    public static void MapRolesEndpoints(this WebApplication app)
    {
        app
            .MapGroup("/api/roles")
            .MapEndpointsApi()
            .WithTags("Roles");
    }

    private static RouteGroupBuilder MapEndpointsApi(this RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", async (
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var roles = await dbContext.Roles.ToListAsync();

            return Results.Ok(roles);
        });
        
        groupBuilder.MapGet("/{id:long}", async (
            [FromRoute] long id,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);

            if (role is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(role);
        });

        groupBuilder.MapPost("/", async (
            [FromServices] IApplicationDbContextFactory dbContextFactory,
            [FromServices] IRabbitMqConnection rabbitMqConnection) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
        
            var role = new Faker<Role>()
                .RuleFor(r => r.Id, _ => 0)
                .RuleFor(r => r.Name, f => f.Name.JobType())
                .Generate();
        
            dbContext.Roles.Add(role);
        
            await dbContext.SaveChangesAsync();
            
            // Publish message
            await rabbitMqConnection.CreateConnectionAsync();
            await rabbitMqConnection.PublishAsync(
                "svc_message_bus",
                "routing.event.audit.create.role",
                role);
        
            return TypedResults.Created($"api/role/{role.Id}");
        });

        groupBuilder.MapDelete("/{id:long}", async (
            [FromRoute] long id,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            await dbContext.Roles
                .Where(r => r.Id == id)
                .DeleteFromQueryAsync();

            return Results.Ok();
        });

        return groupBuilder;
    }
}