using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SVC.Api.EndPoints;
using SVC.Infrastructure;
using SVC.Infrastructure.DataAccess;
using SVC.SharedKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.RegisterInfrastructureDependencies();
builder.Services.RegisterRabbitMqDependencies();

// builder.Services.AddHostedService<GracePeriodManagerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.DefaultFonts = false;
        options
            .WithTitle("Employee Management System (EMS)")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Http);
    });
}

app.UseHttpsRedirection();

app.MapEmployeesEndpoints();
app.MapRolesEndpoints();
app.MapPlatoonEndpoints();

app.Run();