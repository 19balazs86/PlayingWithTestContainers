using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;

namespace WebAPI.Data;

public sealed class WebApiContext : DbContext
{
    private static readonly Func<WebApiContext, string, Task<Member?>> _getMemberByEmail =
        EF.CompileAsyncQuery((WebApiContext context, string email) =>
            context.Members.AsNoTracking().FirstOrDefault(m => m.Email == email));

    public DbSet<Member> Members { get; set; } = default!;

    public WebApiContext(DbContextOptions options) : base(options)
    {
    }

    public async Task<Member?> GetMemberByEmailAsync(string email)
    {
        // Compiled Queries
        // https://www.milanjovanovic.tech/blog/unleash-ef-core-performance-with-compiled-queries

        return await _getMemberByEmail(this, email);
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
