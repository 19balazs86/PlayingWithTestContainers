using Carter;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WebAPI.Data;
using WebAPI.Infrastructure;

namespace WebAPI;

public sealed class Program
{
    private const string _endpointNotFound = "The requested endpoint is not found.";

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

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errrors?view=aspnetcore-7.0#problem-details
            services.AddProblemDetails();

            // For better performance use AddDbContextPool instead of AddDbContext
            // Logging configuration can be set here using the options or in the WebApiContext
            services.AddDbContextPool<WebApiContext>(options => options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")));

            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.PropertyNamingPolicy = null); // null makes is PascalCase. Default: JsonNamingPolicy.CamelCase;

            services.AddHostedService<MigrationBackgroundService>(); // This is not suggested in prod.
        }

        WebApplication app = builder.Build();

        // Configure the request pipeline
        {
            app.UseHttpsRedirection();

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.MapCarter();

            app.MapFallback(endpointNotFoundHandler);
        }

        app.Run();
    }

    private static IResult endpointNotFoundHandler()
        => TypedResults.Problem(_endpointNotFound, statusCode: Status404NotFound);
}