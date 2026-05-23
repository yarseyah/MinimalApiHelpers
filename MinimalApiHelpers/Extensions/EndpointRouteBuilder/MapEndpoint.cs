using Builder = Microsoft.AspNetCore.Builder;

namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Register an <see cref="IEndpoint"/> implementation on the builder.
    /// If the endpoint declares <see cref="IEndpoint.AuthorizationPolicy"/>,
    /// <c>.RequireAuthorization(policy)</c> is automatically chained.
    /// </summary>
    /// <returns>The <see cref="Microsoft.AspNetCore.Builder.RouteHandlerBuilder"/> returned by the
    /// endpoint's <c>Map</c> method, so further fluent configuration (e.g. <c>.WithUserRole(...)</c>)
    /// can be chained.</returns>
    public static Builder.RouteHandlerBuilder MapEndpoint<TEndpoint>(
        this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        var handler = TEndpoint.Map(app);

        if (TEndpoint.AuthorizationPolicy is { Length: > 0 } policy)
            handler.RequireAuthorization(policy);

        return handler;
    }
}