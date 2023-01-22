namespace WebAPI.Data;

public readonly record struct MemberId(int Value)
{
    public static MemberId Create(int Value) => new MemberId(Value);

    public static implicit operator int(MemberId memberId) => memberId.Value;

    //public static explicit operator MemberId(int value) => new MemberId(value);

    public override string ToString() => Value.ToString();
}

public sealed class Member : BaseEntityWithId<MemberId>
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public MembershipEnum Membership { get; set; } = MembershipEnum.Basic;

    public List<PaymentTypeEnum> PaymentTypes { get; set; } = PaymentTypeEnum.FromValue(PaymentTypeEnum.None).ToList();

    public ContactDetails? ContactDetails { get; set; }

    public ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public static string CalculatePaymentTypesText(IEnumerable<PaymentTypeEnum> paymentTypes)
    {
        if (paymentTypes?.Any() is null or false)
            return PaymentTypeEnum.None.Name;

        return paymentTypes.Contains(PaymentTypeEnum.All) ?
            PaymentTypeEnum.All.Name :
            PaymentTypeEnum.FromValueToString(paymentTypes.Distinct().Sum(x => x.Value));
    }
}

public sealed class ContactDetails
{
    public string? Phone { get; set; }
    public Address? Address { get; set; }
}

public sealed class Address
{
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}