namespace MinimalApiHelpers.FluentValidation.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapProtectedGroupWithValidation(
        this IEndpointRouteBuilder app,
        string? prefix = null)
        => app.MapGroup(prefix ?? string.Empty)
            .AddDefaultFiltersIncludingValidation();
}