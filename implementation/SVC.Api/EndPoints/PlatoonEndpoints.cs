using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVC.Core.Entities;
using SVC.Infrastructure.DataAccess;
using SVC.SharedKernel;

namespace SVC.Api.EndPoints;

public static class PlatoonEndpoints
{
    public static void MapPlatoonEndpoints(this WebApplication app)
    {
        app
            .MapGroup("/api/platoons")
            .MapEndpointsApi()
            .WithTags("Platoons");
    }

    private static RouteGroupBuilder MapEndpointsApi(this RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", async (
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var platoons = await dbContext.Platoons.ToListAsync();

            return Results.Ok(platoons);
        });
        
        groupBuilder.MapGet("/{id:long}", async (
            [FromRoute] long id,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var platoon = await dbContext.Platoons.FirstOrDefaultAsync(r => r.Id == id);

            if (platoon is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(platoon);
        });

        groupBuilder.MapPost("/", async (
            [FromServices] IApplicationDbContextFactory dbContextFactory,
            [FromServices] IRabbitMqConnection rabbitMqConnection) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
        
            var platoon = new Faker<Platoon>()
                .RuleFor(r => r.Id, _ => 0)
                .RuleFor(p => p.Name, p => p.Company.CompanyName())
                .RuleFor(p => p.Description, p => p.Lorem.Word())
                .Generate();
        
            dbContext.Platoons.Add(platoon);
        
            await dbContext.SaveChangesAsync();
            
            // Publish message
            await rabbitMqConnection.CreateConnectionAsync();
            await rabbitMqConnection.PublishAsync(
                "svc_message_bus",
                "routing.event.audit.create.platoon",
                platoon);
        
            return TypedResults.Created($"api/platoons/{platoon.Id}");
        });

        groupBuilder.MapDelete("/{id:long}", async (
            [FromRoute] long id,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            await dbContext.Platoons
                .Where(r => r.Id == id)
                .DeleteFromQueryAsync();

            return Results.Ok();
        });

        return groupBuilder;
    }
}