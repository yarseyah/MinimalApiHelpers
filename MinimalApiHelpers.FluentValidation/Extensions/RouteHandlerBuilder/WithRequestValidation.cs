namespace MinimalApiHelpers.FluentValidation.Extensions.RouteHandlerBuilder;

using RouteHandlerBuilder = Microsoft.AspNetCore.Builder.RouteHandlerBuilder;

public static partial class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a request validation filter to the route handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>
    /// A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static RouteHandlerBuilder WithRequestValidation<TRequest>(
        this RouteHandlerBuilder builder)
        => builder
            .AddEndpointFilter<RequestValidationFilter<TRequest>>()
            .ProducesValidationProblem();
}