using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebAPI.Data;

public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        // Id
        builder.Property(m => m.Id)
            .HasConversion(memberId => memberId.Value, value => MemberId.Create(value))
            .ValueGeneratedOnAdd();

        // Name
        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(50);

        // Email
        builder.HasIndex(m => m.Email).IsUnique();

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(50);

        // IsDeleted - Add an index for better query performance
        builder.HasIndex(m => m.IsDeleted)
            // Specify a filtered to index only not deleted values, reducing the index's size and disk space usage
            // The expression is different between the DB providers. Feel free to ignore the HasFilter.
            .HasFilter("\"IsDeleted\" = false");


        // ContactDetails
        builder.OwnsOne(member =>
            member.ContactDetails, ownedNavigationBuilder =>
            {
                ownedNavigationBuilder.ToJson();
                ownedNavigationBuilder.OwnsOne(contactDetails => contactDetails.Address);
            });

        // You can use the new EF8 Complex types feature, but JSON is more simple
        // https://devblogs.microsoft.com/dotnet/announcing-ef8-rc1/
        // https://learn.microsoft.com/hu-hu/ef/core/what-is-new/ef-core-8.0/whatsnew#value-objects-using-complex-types
        // builder.ComplexProperty(m => m.ContactDetails, innerCompPropBuilder => innerCompPropBuilder.ComplexProperty(cd => cd.Address));

        // builder.Property(m => m.ContactDetails).HasColumnType("jsonb"); // Deprecated

        // Membership
        builder.Property(m => m.Membership)
            .HasConversion(membership => membership.Value, v => MembershipEnum.FromValue(v));

        // PaymentTypes
        var valueComparer = new ValueComparer<List<PaymentTypeEnum>>(
            (pt, pt2) => pt!.SequenceEqual(pt2!),
            pt        => pt.Distinct().Aggregate(0, (hashCode, pt) => HashCode.Combine(hashCode, pt.Value)),
            pt        => pt);

        builder.Property(m => m.PaymentTypes)
            .HasConversion(
                paymentTypes => Member.CalculatePaymentTypesText(paymentTypes),
                names        => PaymentTypeEnum.FromName(names, false, false).ToList(),
                valueComparer)
            .HasMaxLength(50);
    }
}