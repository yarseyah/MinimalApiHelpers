namespace Demo.Apis;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var validated = builder.MapPublicGroup("/validated")
            .WithTags("Validated")
            .AddValidationFilters();

        validated.MapEndpoint<Validated.ValidatedPing>();
        validated.MapEndpoint<Common.Pong>();

        var unvalidated = builder.MapPublicGroup("/unvalidated")
            .WithTags("Unvalidated");

        unvalidated.MapEndpoint<Unvalidated.UnvalidatedPing>();
        unvalidated.MapEndpoint<Common.Pong>();

        return builder;
    }
}