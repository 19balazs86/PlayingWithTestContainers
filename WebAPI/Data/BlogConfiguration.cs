using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebAPI.Data;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        // Id
        builder.Property(b => b.Id)
            .HasConversion(courseId => courseId.Value, value => BlogId.Create(value))
            .ValueGeneratedOnAdd();

        // Title
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(50);

        // 1 Owner has many Blogs
        builder
            .HasOne(b => b.Owner)
            .WithMany(m => m.Blogs)
            .HasForeignKey(b => b.OwnerId)
            .OnDelete(DeleteBehavior.Cascade); // Default: Cascade

        // Tags
        // List<string> type will be 'text[]' in PostgreSQL
        // builder.Property(b => b.Tags).HasColumnType("jsonb"); // Deprecated

        // IsDeleted - Add an index for better query performance
        builder.HasIndex(b => b.IsDeleted)
            // Specify a filtered to index only not deleted values, reducing the index's size and disk space usage
            // The expression is different between the DB providers. Feel free to ignore the HasFilter.
            .HasFilter("\"IsDeleted\" = false");
        // https://learn.microsoft.com/en-us/ef/core/modeling/indexes?tabs=fluent-api#index-filter
    }
}