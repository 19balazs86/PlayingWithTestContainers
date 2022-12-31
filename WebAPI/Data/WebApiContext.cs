using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace WebAPI.Data;

public sealed class WebApiContext : DbContext
{
    public DbSet<Member> Members { get; set; } = default!;

    public WebApiContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebApiContext).Assembly);
        modelBuilder.ApplyConfiguration(new MemberConfiguration());

        setGlobalFilterToAllEntitiesForSoftDelete(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        DateTime utcNow = DateTime.UtcNow;

        ChangeTracker
            .Entries<BaseEntity>()
            .Where(entity => entity.State == EntityState.Added)
            .ToList()
            .ForEach(entity => entity.Entity.CreatedAt = utcNow);

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private static void setGlobalFilterToAllEntitiesForSoftDelete(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IMutableProperty? isDeletedProperty = entityType.FindProperty(nameof(BaseEntity.DeletedAt));

            if (isDeletedProperty is not null)
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");

                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, isDeletedProperty.PropertyInfo!),
                        Expression.Constant(null, typeof(DateTime?))
                    ), parameter);

                entityType.SetQueryFilter(filter);
            }
        }
    }
}
