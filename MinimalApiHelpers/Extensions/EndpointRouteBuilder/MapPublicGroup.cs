namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapPublicGroup(
        this IEndpointRouteBuilder app,
        string? prefix = null)
        => app.MapGroup(prefix ?? string.Empty)
            .AllowAnonymous()
            .AddDefaultFilters();
}