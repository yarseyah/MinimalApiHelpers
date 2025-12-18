namespace MinimalApiHelpers.FluentValidation.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    [UsedImplicitly]
    public static RouteGroupBuilder AddDefaultFiltersIncludingValidation(this RouteGroupBuilder app)
        => app
            .AddDefaultFilters()
            .AddEndpointFilter<ValidationExceptionFilter>();
}