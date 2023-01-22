using FluentValidation;
using WebAPI.DTOs;

namespace WebAPI.Validations;

public sealed class CreateBlogRequestValidator : AbstractValidator<BlogDTO>
{
    public CreateBlogRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.OwnerId)
            .GreaterThan(0);
    }
}
