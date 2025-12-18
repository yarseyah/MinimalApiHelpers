namespace Demo;

[UsedImplicitly]
public sealed class UnvalidatedPingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/ping", Handler);

    private static Ok<string> Handler(
        [AsParameters] Request request) 
        => TypedResults.Ok($"Pong: {request.Id}");

    [UsedImplicitly]
    public sealed record Request(
        string? Id
    );
}