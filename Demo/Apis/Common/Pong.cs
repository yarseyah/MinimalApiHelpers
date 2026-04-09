namespace Demo.Apis.Common;

[UsedImplicitly]
public sealed class Pong : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/pong", Handle)
            .WithName(nameof(Pong))
            .WithSummary("Return a static pong response.")
            .Produces<Ok<string>>();

    private static ValueTask<Ok<string>> Handle()
        => ValueTask.FromResult(TypedResults.Ok("pong"));
}