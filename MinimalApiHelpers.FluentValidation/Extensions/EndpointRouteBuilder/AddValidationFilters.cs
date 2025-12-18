namespace MinimalApiHelpers.FluentValidation.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    [UsedImplicitly]
    public static RouteGroupBuilder AddValidationFilters(this RouteGroupBuilder app)
        => app
            .AddEndpointFilter<ValidationExceptionFilter>();
}