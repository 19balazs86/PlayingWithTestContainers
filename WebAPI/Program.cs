using FluentValidation;
using Mapster;
using System.Reflection;
using WebAPI.Endpoints;
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
            services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errrors?view=aspnetcore-7.0#problem-details
            services.AddProblemDetails();

            services.AddDbContextWithInterceptors();

            services.ConfigureHttpJsonOptions(options => options.SerializerOptions.PropertyNamingPolicy = null); // null makes is PascalCase. Default: JsonNamingPolicy.CamelCase;

            services.AddHostedService<MigrationBackgroundService>(); // This is not suggested in prod.
        }

        WebApplication app = builder.Build();

        // Configure the request pipeline
        {
            app.UseHttpsRedirection();

            app.UseExceptionHandler();
            app.UseStatusCodePages();

            app.MapMemberEndpoints();
            app.MapBlogEndpoints();

            app.MapFallback(endpointNotFoundHandler);
        }

        app.Run();
    }

    private static IResult endpointNotFoundHandler()
        => TypedResults.Problem(_endpointNotFound, statusCode: Status404NotFound);
}