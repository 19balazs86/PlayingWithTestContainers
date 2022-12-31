using Carter;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using WebAPI.Data;
using WebAPI.Migrations;

namespace WebAPI;

public sealed class Program
{
    public static void Main(string[] args)
    {
        // Mapster
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        var configuration = builder.Configuration;
        var services      = builder.Services;

        // Add services to the container
        {
            // I do not use the build-in validation function. I created a FilterFactory in the ValidationFilter class.
            services.AddCarter(configurator: configuration => configuration.WithEmptyValidators());

            services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

            services.AddProblemDetails();

            services.AddDbContext<WebApiContext>(options => options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.PropertyNamingPolicy = null); // null makes is PascalCase. Default: JsonNamingPolicy.CamelCase;

            services.AddHostedService<MigrationBackgroundService>(); // This is not suggested in prod.
        }

        WebApplication app = builder.Build();

        // Configure the request pipeline
        {
            app.UseHttpsRedirection();

            app.UseExceptionHandler();

            app.MapCarter();

            app.MapFallback(pageNotFoundHandler);
        }

        app.Run();
    }

    private static async Task pageNotFoundHandler(HttpContext context)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode  = Status404NotFound;

        var problemDetails = new ProblemDetails
        {
            Title = "The requested endpoint is not found.",
            Status = Status404NotFound,
        };

        await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails);
    }
}