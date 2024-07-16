using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebAPI.Data;
using WebAPI.Infrastructure.Interceptors;

namespace WebAPI.Infrastructure;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextWithInterceptors(this IServiceCollection services)
    {
        services.AddSingleton<BaseEntityInterceptor>();
        services.AddSingleton<BaseEntityExecuteDeleteInterceptor>();

        // For better performance use AddDbContextPool instead of AddDbContext
        services.AddDbContextPool<WebApiContext>((serviceProvider, options) =>
        {
            // Configuration is acquired in the AddDbContextPool. Otherwise, the integration test fails to find the ConnectionString
            string connectionString = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetConnectionString("PostgreSQL")
                ?? throw new NullReferenceException("Missing ConnectionString: 'PostgreSQL'");

            options.UseNpgsql(connectionString);

            // Set QueryTracking to AsNoTracking() by default, but you can configure it individually in IEntityTypeConfiguration
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            // --> Add: Interceptors
            IInterceptor[] interceptors =
            [
                serviceProvider.GetRequiredService<BaseEntityInterceptor>(),
                serviceProvider.GetRequiredService<BaseEntityExecuteDeleteInterceptor>()
            ];

            options.AddInterceptors(interceptors);

            // --> Logging
            // With sensitive logging, you can see the parameter values in the SQL query
            //options.EnableSensitiveDataLogging();
            // You can write the SQL query directly to the console, OR you can set the logging configuration to "Microsoft.EntityFrameworkCore": "Information"
            //options.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information);
        });

        return services;
    }
}
