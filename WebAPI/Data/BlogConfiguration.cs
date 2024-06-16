using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebAPI.Data;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        // Id
        builder.Property(c => c.Id)
            .HasConversion(courseId => courseId.Value, value => BlogId.Create(value))
            .ValueGeneratedOnAdd();

        // Name
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(50);

        // 1 Owner has many Blogs
        builder
            .HasOne(c => c.Owner)
            .WithMany(s => s.Blogs)
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // Default: Cascade

        // Tags
        // List<string> type will be 'text[]' in PostgreSQL
        // builder.Property(m => m.Tags).HasColumnType("jsonb"); // Deprecated

        // IsDeleted
        // Add an index for faster queries, filtering for deleted records. The expression is different between the DB providers. Feel free to ignore the HasFilter.
        // builder.HasIndex(b => b.IsDeleted).HasFilter("is_deleted = 0");
        // https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=fluent-api#index-filter
    }
}