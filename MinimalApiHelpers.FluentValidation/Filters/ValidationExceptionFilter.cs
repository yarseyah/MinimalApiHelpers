namespace MinimalApiHelpers.FluentValidation.Filters;

public class ValidationExceptionFilter(
    NotFoundErrorCodeMapping? notFoundMapping = null) : IEndpointFilter
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
        catch (ValidationException ex)
        {
            if (notFoundMapping is not null
                && notFoundMapping.Count > 0
                && ex.Errors.All(e => notFoundMapping.Contains(e.ErrorCode)))
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