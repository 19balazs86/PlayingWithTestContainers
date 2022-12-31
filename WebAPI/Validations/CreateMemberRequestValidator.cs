using FluentValidation;
using WebAPI.Data;
using WebAPI.DTOs;

namespace WebAPI.Validations;

public sealed class CreateMemberRequestValidator : AbstractValidator<MemberDTO>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(dateOfBirth => DateOnly.TryParseExact(dateOfBirth, "yyyy-MM-dd", out var _))
            .WithMessage("'{PropertyName}' is invalid. Accepted format: YYYY-MM-DD");

        RuleFor(x => x.Membership)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(membership => MembershipEnum.TryFromName(membership, true, out var _))
            .WithMessage("'{PropertyName}' is invalid. Accepted values: " + string.Join(", ", MembershipEnum.List));

        RuleFor(x => x.PaymentTypes)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(paymentTypes =>
                PaymentTypeEnum.TryFromName(paymentTypes, true, out IEnumerable<PaymentTypeEnum> paymentTypesList) &&
                paymentTypesList.Count() == paymentTypes.Split(",").Count())
            .WithMessage("'{PropertyName}' is invalid. Accepted values: " + string.Join(", ", PaymentTypeEnum.List));
    }
}
