namespace MinimalApiHelpers.Filters;

using MinimalApiHelpers.Extensions.RouteHandlerBuilder;

public class EntityExistsVerificationFailureExceptionFilter(
    ILogger<EntityExistsVerificationFailureExceptionFilter> logger)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next(context);
        }
        catch (LookedUpEntityWasNotActuallyFoundException lookupFailedException)
        {
            logger.LogError(
                lookupFailedException,
                "The '{FilterName}' middleware claimed resource existed, but the service itself returned NotFound",
                nameof(RouteHandlerBuilderExtensions.WithEnsureEntityExists));
            return Results.NotFound();
        }
    }
}