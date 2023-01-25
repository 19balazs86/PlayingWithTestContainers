using FluentValidation;
using System.Reflection;

namespace WebAPI.Validations;

[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ValidateAttribute : Attribute
{
}

// Based on Ben Foster blog post: https://benfoster.io/blog/minimal-api-validation-endpoint-filters
// Nuget package: https://www.nuget.org/packages/O9d.AspNet.FluentValidation
public static class ValidationEndpointFilterFactory
{
    public static EndpointFilterDelegate Create(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        var validationDescriptors = getValidatonDescriptors(context.MethodInfo).ToList();

        if (validationDescriptors.Count > 0)
            return invocationContext => validate(validationDescriptors, invocationContext, next);

        return invocationContext => next(invocationContext);
    }

    private static async ValueTask<object?> validate(
        IEnumerable<ValidationDescriptor> validationDescriptors,
        EndpointFilterInvocationContext invocationContext,
        EndpointFilterDelegate next)
    {
        foreach (ValidationDescriptor descriptor in validationDescriptors)
        {
            object? argument = invocationContext.Arguments[descriptor.ArgumentIndex];

            if (argument is not null)
            {
                Type validatorType = typeof(IValidator<>).MakeGenericType(descriptor.ArgumentType);

                IValidator? validator = invocationContext.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator is not null)
                {
                    var validationResult = await validator.ValidateAsync(new ValidationContext<object>(argument));

                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary(), statusCode: Status422UnprocessableEntity);
                    }
                }
            }
        }

        return await next.Invoke(invocationContext);
    }

    private static IEnumerable<ValidationDescriptor> getValidatonDescriptors(MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];

            if (parameter.GetCustomAttribute<ValidateAttribute>() is not null)
            {
                yield return new ValidationDescriptor(index, parameter.ParameterType);
            }
        }
    }

    private readonly record struct ValidationDescriptor(int ArgumentIndex, Type ArgumentType);
}
