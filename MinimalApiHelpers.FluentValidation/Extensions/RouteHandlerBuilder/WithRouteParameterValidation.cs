namespace MinimalApiHelpers.FluentValidation.Extensions.RouteHandlerBuilder;

using RouteHandlerBuilder = Microsoft.AspNetCore.Builder.RouteHandlerBuilder;

public static partial class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a route parameter validation filter to the route handler.
    /// </summary>
    /// <typeparam name="TParam">The type of the parameter to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns></returns>
    public static RouteHandlerBuilder WithRouteParameterValidation<TParam>(
        this RouteHandlerBuilder builder) =>
        builder
            .AddEndpointFilter<RouteParameterValidationFilter<TParam>>()
            .ProducesValidationProblem();
}