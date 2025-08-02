namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapEndpoint<TEndpoint>(
        this IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint
    {
        TEndpoint.Map(app);
        return app;
    }
}