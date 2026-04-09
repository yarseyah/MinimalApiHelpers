namespace Demo.Apis;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPublicGroup("/validated")
            .WithTags("Validated")
            .AddValidationFilters()
            .MapEndpoint<Validated.ValidatedPing>()
            .MapEndpoint<Common.Pong>();

        builder.MapPublicGroup("/unvalidated")
            .WithTags("Unvalidated")
            .MapEndpoint<Unvalidated.UnvalidatedPing>()
            .MapEndpoint<Common.Pong>();

        return builder;
    }
}