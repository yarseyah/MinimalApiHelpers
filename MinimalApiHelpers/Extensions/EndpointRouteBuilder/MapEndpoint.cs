namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

using Builder = Microsoft.AspNetCore.Builder;

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
        return (TEndpoint.AuthorizationPolicy is { Length: > 0 } policy)
            ? TEndpoint.Map(app).RequireAuthorization(policy)
            : TEndpoint.Map(app);
    }
}