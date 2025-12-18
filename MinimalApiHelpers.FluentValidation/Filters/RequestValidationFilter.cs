namespace MinimalApiHelpers.FluentValidation.Filters;

[UsedImplicitly]
public class RequestValidationFilter<TRequest>(
    ILogger<RequestValidationFilter<TRequest>> logger,
    IValidator<TRequest>? validator = null) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var requestName = typeof(TRequest).FullName;
        if (validator is null)
        {
            logger.LogError("{Request}: No validator configured", requestName);
            return await next(context);
        }

        logger.LogDebug("{Request}: Validating using {Validator}", requestName, validator.GetType().FullName);
        
        var request = context.Arguments.OfType<TRequest>().First();
        var validationResult =
            await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!validationResult.IsValid)
        {
            logger.LogWarning("{Request}: Validation failed", requestName);
            foreach (var error in validationResult.Errors)
            {
                logger.LogTrace("Validation error: {@Error}", error);
            }
            
            logger.LogWarning(
                "TODO: rather than throwing, share the same ProblemReport construction as the exception " +
                "handler and return as TypedResults.ValidationException");
            throw new ValidationException(validationResult.Errors);
        }

        logger.LogDebug("{Request}: Validation succeeded", requestName);
        return await next(context);
    }
}