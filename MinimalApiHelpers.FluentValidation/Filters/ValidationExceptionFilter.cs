namespace MinimalApiHelpers.FluentValidation.Filters;

public class ValidationExceptionFilter : IEndpointFilter
{
    private static readonly string[] ErrorCodesToConvertToNotFound =
    [
        "team-not-found",
        "season-not-found"
    ];

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
        catch (ValidationException ex)
        {
            // Convert specific errors to a NotFound response
            if (ex.Errors.All(e => ErrorCodesToConvertToNotFound.Contains(e.ErrorCode)))
            {
                return Results.NotFound();
            }

            // Convert the exception to a ProblemDetails response
            return Results.Problem(
                new ProblemDetails
                {
                    Title = "Bad Request",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://rfc-editor.org/rfc/rfc7231#section-6.5.1",
                    Instance = context.HttpContext.Request.Path,
                    Extensions =
                    {
                        ["traceId"] = context.HttpContext.TraceIdentifier,
                        ["errors"] = ex.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray()),
                    },
                });
        }
    }
}