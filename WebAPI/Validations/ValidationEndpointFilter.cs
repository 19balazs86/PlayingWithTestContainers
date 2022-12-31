using FluentValidation;

namespace WebAPI.Validations;

public sealed class ValidationEndpointFilter<TModel> : IEndpointFilter where TModel : class
{
    private readonly IValidator<TModel> _validator;

    public ValidationEndpointFilter(IValidator<TModel> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator is not null)
        {
            Type modelTyp = typeof(TModel);

            TModel? model = context.Arguments.FirstOrDefault(x => x?.GetType() == modelTyp) as TModel;

            if (model is not null)
            {
                var validationResult = await _validator.ValidateAsync(model);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary(), statusCode: Status400BadRequest);
                }
            }
        }

        return await next(context);
    }
}
