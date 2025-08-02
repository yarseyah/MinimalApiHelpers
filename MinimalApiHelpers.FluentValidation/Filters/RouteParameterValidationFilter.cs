namespace MinimalApiHelpers.FluentValidation.Filters;

public class RouteParameterValidationFilter<TParam>(
    ILogger<RouteParameterValidationFilter<TParam>> logger,
    IValidator<TParam>? validator = null) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var paramName = typeof(TParam).FullName;
        if (validator is null)
        {
            logger.LogError("{Param}: No validator configured", paramName);
            return await next(context);
        }

        var param = context.Arguments.OfType<TParam>().FirstOrDefault();
        if (param is null)
        {
            logger.LogError("{Param}: No parameter of type {Type} found in arguments", paramName, typeof(TParam));
            return await next(context);
        }

        logger.LogDebug("{Param}: Validating using {Validator}", paramName, validator.GetType().FullName);

        var validationResult = await validator.ValidateAsync(param, context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("{Param}: Validation failed", paramName);
            foreach (var error in validationResult.Errors)
            {
                logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
            }
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        return await next(context);
    }
}