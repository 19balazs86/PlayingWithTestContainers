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
                    // 400 vs 422 for Client Error Request
                    // https://stackoverflow.com/questions/51990143/400-vs-422-for-client-error-request/52098667#52098667

                    return Results.ValidationProblem(validationResult.ToDictionary(), statusCode: Status422UnprocessableEntity);
                }
            }
        }

        return await next(context);
    }
}
