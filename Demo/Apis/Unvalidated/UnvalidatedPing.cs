namespace Demo.Apis.Unvalidated;

[UsedImplicitly]
public sealed class UnvalidatedPing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/ping", Handle)
            .WithName(nameof(UnvalidatedPing))
            .WithSummary("Return a pong response without request validation.")
            .Produces<Ok<string>>();

    private static ValueTask<Ok<string>> Handle([AsParameters] Request request)
        => ValueTask.FromResult(TypedResults.Ok($"Pong: {request.Id}"));

    [UsedImplicitly]
    public sealed record Request(string? Id);
}