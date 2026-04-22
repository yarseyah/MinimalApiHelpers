namespace MinimalApiHelpers.Extensions.RouteHandlerBuilder;

using RouteHandlerBuilder = Microsoft.AspNetCore.Builder.RouteHandlerBuilder;

public static partial class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds an endpoint filter that returns <c>409 Conflict</c> (ProblemDetails) before the handler
    /// runs if <paramref name="entityExists"/> returns <c>true</c>.
    /// This is the symmetric counterpart to <c>WithEnsureEntityExists</c> (which returns 404).
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="entityExists">The function that determines if the entity exists.</param>
    /// <param name="conflictDetail">The detail to include in the ProblemDetails.</param>
    /// <returns>The route handler builder.</returns>
    internal static RouteHandlerBuilder WithConflictIfEntityExists<TRequest, TService>(
        this RouteHandlerBuilder builder,
        Func<HttpContext, TRequest, TService, CancellationToken, Task<bool>> entityExists,
        Func<HttpContext, TRequest, string>? conflictDetail = null)
        where TRequest : class
        where TService : class
        => builder.AddEndpointFilter(async (ctx, next) => {
                var svc = ctx.HttpContext.RequestServices.GetRequiredService<TService>();
                var req = ctx.Arguments.OfType<TRequest>().FirstOrDefault();

                if (req is not null && await entityExists(ctx.HttpContext, req, svc, ctx.HttpContext.RequestAborted))
                {
                    var detail = conflictDetail?.Invoke(ctx.HttpContext, req) ?? "A resource with the specified parameters already exists.";

                    return Results.Problem(
                        detail: detail,
                        statusCode: StatusCodes.Status409Conflict,
                        title: "Conflict");
                }

                return await next(ctx);
            })
            .ProducesProblem(StatusCodes.Status409Conflict);
}
