namespace MinimalApiHelpers.Filters;

public class InternalServerErrorExceptionFilter(
    ILogger<InternalServerErrorExceptionFilter> logger)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            return await next(context);
        }
        catch (InternalServerErrorException ex)
        {
            // Generate GUID v7
            var exceptionId = Guid.CreateVersion7();
            logger.LogError(ex, "An error occurred while processing the request: {ExceptionId}", exceptionId);

            return Results.Problem(
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://rfc-editor.org/rfc/rfc7231#section-6.6.1",
                    Instance = context.HttpContext.Request.Path,
                    Extensions =
                    {
                        ["traceId"] = context.HttpContext.TraceIdentifier,
                        ["exceptionId"] = exceptionId,
                        ["errors"] = new Dictionary<string, string[]>
                        {
                            ["message"] =
                            [
                                "An error occurred while processing the request.",
                                $"If contacting support, please provide the following exception ID: {exceptionId}"
                            ],
                        },
                    },
                });
        }
    }
}