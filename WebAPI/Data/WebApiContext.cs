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

    public DbSet<Blog> Blogs { get; set; } = default!;

    public WebApiContext(DbContextOptions options) : base(options)
    {

    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    // This configuration is unnecessary if the Default LogLevel is set to Information, which is suitable for Development
    //    // It can also be configured in the program when you add the DbContext
    //    // Note: 'OnConfiguring' cannot be used to modify DbContextOptions when DbContext pooling is enabled.
    //    optionsBuilder
    //        .LogTo(Console.WriteLine, LogLevel.Information)
    //        .EnableSensitiveDataLogging()
    //        .EnableDetailedErrors();
    //}

    public async Task<Member?> GetMemberByEmailAsync(string email)
    {
        // Compiled Queries
        // Milan: https://www.milanjovanovic.tech/blog/unleash-ef-core-performance-with-compiled-queries
        // Nick Chapsas - Making EF as fast as Dapper: https://youtu.be/OxqAUIYemMs

        return await _getMemberByEmail(this, email);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(WebApiContext).Assembly);
        modelBuilder.ApplyConfiguration(new MemberConfiguration());
        modelBuilder.ApplyConfiguration(new BlogConfiguration());

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
