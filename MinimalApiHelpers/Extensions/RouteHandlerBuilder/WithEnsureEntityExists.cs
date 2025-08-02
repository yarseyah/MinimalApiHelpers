namespace MinimalApiHelpers.Extensions.RouteHandlerBuilder;

using RouteHandlerBuilder = Microsoft.AspNetCore.Builder.RouteHandlerBuilder;

public static partial class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder
        WithEnsureEntityExists<TRequest, TExistsLookup>(
            this RouteHandlerBuilder builder,
            Func<HttpContext, TRequest, TExistsLookup, CancellationToken, ValueTask<bool>> entityExists)
        where TRequest : notnull
        where TExistsLookup : notnull
    {
        return builder.AddEndpointFilterFactory(
                (_, next) => async context =>
                {
                    var request = context.Arguments.OfType<TRequest>().Single();
                    var cancellationToken = context.HttpContext.RequestAborted;
                    var httpContext = context.HttpContext;

                    var lookup = context.HttpContext.RequestServices
                        .GetRequiredService<TExistsLookup>();

                    return await entityExists(
                        httpContext,
                        request,
                        lookup,
                        cancellationToken)
                        ? await next(context)
                        : Results.NotFound();
                })
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}