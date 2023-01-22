﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebAPI.Data;

public sealed class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        // Id
        builder.Property(c => c.Id)
            .HasConversion(courseId => courseId.Value, value => new BlogId(value))
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
    }
}