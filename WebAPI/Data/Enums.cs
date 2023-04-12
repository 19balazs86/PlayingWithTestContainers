using Ardalis.SmartEnum;
using System.Diagnostics;

namespace WebAPI.Data;

// There is another package called Intellenum, which uses source generation.
// https://github.com/SteveDunn/Intellenum

[DebuggerDisplay("Valu = {Value}, Name = {Name}, Discount = {DiscountPercent}")]
public sealed class MembershipEnum : SmartEnum<MembershipEnum>
{
    public static readonly MembershipEnum Basic    = new (nameof(Basic),    1, discountPercent: 0.00);
    public static readonly MembershipEnum Standard = new (nameof(Standard), 2, discountPercent: 0.10);
    public static readonly MembershipEnum Premium  = new (nameof(Premium),  3, discountPercent: 0.25);

    public double DiscountPercent { get; }

    public MembershipEnum(string name, int value, double discountPercent) : base(name, value)
    {
        DiscountPercent = discountPercent;
    }

    public MembershipEnum() : base("NaN", 0) { } // Need this for EF
}


[DebuggerDisplay("Valu = {Value}, Name = {Name}")]
public class PaymentTypeEnum : SmartFlagEnum<PaymentTypeEnum>
{
    public static readonly PaymentTypeEnum All          = new (nameof(All),         -1);
    public static readonly PaymentTypeEnum None         = new (nameof(None),         0);
    public static readonly PaymentTypeEnum Card         = new (nameof(Card),         1);
    public static readonly PaymentTypeEnum Cash         = new (nameof(Cash),         2);
    public static readonly PaymentTypeEnum Bpay         = new (nameof(Bpay),         4);
    public static readonly PaymentTypeEnum Paypal       = new (nameof(Paypal),       8);
    public static readonly PaymentTypeEnum BankTransfer = new (nameof(BankTransfer), 16);

    public PaymentTypeEnum(string name, int value) : base(name, value)
    {
    }

    public PaymentTypeEnum() : base("NaN", 0) { } // Need this for EF
}