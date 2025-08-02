namespace MinimalApiHelpers.Filters;

public class InternalExceptionsFilter(ILogger<InternalExceptionsFilter> logger)
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
        catch (UnsupportedContentTypeException e)
        {
            logger.LogWarning(e, "Request with an unsupported Content-Type was received");
            return TypedResults.Problem(statusCode: StatusCodes.Status415UnsupportedMediaType);
        }
    }
}