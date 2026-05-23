namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Create a route group that requires an authenticated user, with default filters applied.
    /// </summary>
    public static RouteGroupBuilder MapPrivateGroup(
        this IEndpointRouteBuilder app,
        string? prefix = null)
        => app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization()
            .AddDefaultFilters();

    /// <summary>
    /// Create a route group that requires a named authorization policy, with default filters applied.
    /// </summary>
    public static RouteGroupBuilder MapPrivateGroup(
        this IEndpointRouteBuilder app,
        string? prefix,
        string policyName)
        => app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization(policyName)
            .AddDefaultFilters();
}