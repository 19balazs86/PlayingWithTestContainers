using Microsoft.EntityFrameworkCore;
using WebAPI.Data;

namespace WebAPI.Infrastructure;

public static class EntityFrameworkServiceCollectionExtensions
{
    public static IServiceCollection AddDbContextWithInterceptors(this IServiceCollection services)
    {
        services.AddSingleton<BaseEntityInterceptor>();

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

            options.AddInterceptors(serviceProvider.GetRequiredService<BaseEntityInterceptor>());
        });

        return services;
    }
}
