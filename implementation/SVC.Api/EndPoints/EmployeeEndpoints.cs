using System.ComponentModel;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using SVC.Core;
using SVC.Core.Entities;
using SVC.Infrastructure.DataAccess;
using SVC.SharedKernel;

namespace SVC.Api.EndPoints;

public static class EmployeeEndpoints
{
    public static void MapEmployeesEndpoints(this WebApplication app)
    {
        app
            .MapGroup("/api/employees")
            .MapEndpointsApi()
            .WithTags("Employees");
    }

    private static RouteGroupBuilder MapEndpointsApi(this RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet("/", async (
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var ctx = dbContextFactory.CreateDbContext();
            var employees = await ctx.Employees.ToArrayAsync();
            return Results.Ok(employees);
        });

        groupBuilder
            .MapGet("/{id:long}", async (
                [Description("The unique identifier for the employee.")] 
                [FromRoute] long id,
                [FromServices] IApplicationDbContextFactory dbContextFactory) =>
            {
                await using var ctx = dbContextFactory.CreateDbContext();
                var employee = await ctx.Employees.FirstOrDefaultAsync(e => e.Id == id);

                return employee is null ? Results.NotFound() : Results.Ok(employee);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        groupBuilder.MapPost("/", async (
                [FromServices] IApplicationDbContextFactory dbContextFactory,
                [FromServices] IRabbitMqConnection rabbitMqConnection) =>
            {
                await using var ctx = dbContextFactory.CreateDbContext();
                var employee = new Faker<Employee>()
                    .RuleFor(e => e.Id, _ => 0)
                    .RuleFor(e => e.Name, f => f.Person.FirstName)
                    .RuleFor(e => e.EntryDate, f => f.Date.Past(1, DateTime.Now))
                    .RuleFor(e => e.PlatoonId, f => f.Random.Long(min: 1, max: 5))
                    .RuleFor(e => e.RoleId, f => f.Random.Long(min: 1, max: 3))
                    .Generate();

                ctx.Employees.Add(employee);
                await ctx.SaveChangesAsync();

                // Publish message
                await rabbitMqConnection.CreateConnectionAsync();
                await rabbitMqConnection.PublishAsync(
                    "svc_message_bus",
                    "routing.event.audit.create.employee",
                    employee);

                return TypedResults.Created($"api/platoons/{employee.Id}");
            })
            .WithDescription("Create a new employee");
        ;

        groupBuilder.MapPost("/{id:long}", async (
            [FromRoute] long id,
            [FromBody] EmployeeRequest employee,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var ctx = dbContextFactory.CreateDbContext();
            var employeeEntity = await ctx.Employees.FirstOrDefaultAsync(e => e.Id == id);

            if (employeeEntity is null)
            {
                return Results.NotFound();
            }

            employeeEntity.Name = employee.Name;
            employeeEntity.EntryDate = employee.EntryDate;
            employeeEntity.PlatoonId = employee.PlatoonId;
            employeeEntity.RoleId = employee.RoleId;

            await ctx.SaveChangesAsync();

            return Results.Ok(employeeEntity);
        });

        groupBuilder.MapDelete("/{id:long}", async (
            [Description("The unique identifier for the employee.")]
            [FromRoute] long id,
            [FromServices] IApplicationDbContextFactory dbContextFactory) =>
        {
            await using var ctx = dbContextFactory.CreateDbContext();
            await ctx.Employees.Where(e => e.Id == id).DeleteFromQueryAsync();

            return Results.Ok();
        });

        groupBuilder.MapPut("/{id:long}/active", async (
                [FromRoute] long id,
                [FromQuery] bool active,
                [FromServices] IApplicationDbContextFactory dbContextFactory) =>
            {
                await using var ctx = dbContextFactory.CreateDbContext();
                var employee = await ctx.Employees.FirstOrDefaultAsync(e => e.Id == id);

                if (employee is null)
                {
                    return Results.NotFound();
                }

                employee.IsActive = active;
                await ctx.SaveChangesAsync();

                return Results.NoContent();
            })
            .WithDescription("Update an employee with the specified ID and active status");

        return groupBuilder;
    }
}