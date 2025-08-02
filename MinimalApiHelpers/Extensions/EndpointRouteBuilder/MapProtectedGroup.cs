namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapProtectedGroup(
        this IEndpointRouteBuilder app,
        string? prefix = null)
        => app.MapGroup(prefix ?? string.Empty)
            .AddDefaultFilters();
}