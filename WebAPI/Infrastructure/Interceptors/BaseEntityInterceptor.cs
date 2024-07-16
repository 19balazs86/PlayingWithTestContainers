using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebAPI.Data;

namespace WebAPI.Infrastructure.Interceptors;

// Milan's newsletter: https://www.milanjovanovic.tech/blog/how-to-use-ef-core-interceptors
public sealed class BaseEntityInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<BaseEntityInterceptor> _logger;

    public BaseEntityInterceptor(ILogger<BaseEntityInterceptor> logger)
    {
        // You can get dependencies via constructor, because the interceptor is acquired from the DI container
        _logger = logger;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SavingChanges in BaseEntityInterceptor");

        if (eventData.Context is not null)
        {
            updateEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void updateEntities(DbContext dbContext)
    {
        DateTime utcNow = DateTime.UtcNow;

        dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Where(entity => entity.State == EntityState.Added)
            .ToList()
            .ForEach(entity => entity.Entity.CreatedAt = utcNow);
    }
}
