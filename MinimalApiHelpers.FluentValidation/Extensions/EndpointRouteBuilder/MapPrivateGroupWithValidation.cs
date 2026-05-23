namespace MinimalApiHelpers.FluentValidation.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Create a route group that requires an authenticated user, with default filters and validation applied.
    /// </summary>
    public static RouteGroupBuilder MapPrivateGroupWithValidation(
        this IEndpointRouteBuilder app,
        string? prefix = null)
        => app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization()
            .AddDefaultFiltersIncludingValidation();

    /// <summary>
    /// Create a route group that requires a named authorization policy, with default filters and validation applied.
    /// </summary>
    public static RouteGroupBuilder MapPrivateGroupWithValidation(
        this IEndpointRouteBuilder app,
        string? prefix,
        string policyName)
        => app.MapGroup(prefix ?? string.Empty)
            .RequireAuthorization(policyName)
            .AddDefaultFiltersIncludingValidation();
}