namespace Demo;

[UsedImplicitly]
public sealed class PongEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("pong", () => Results.Ok("pong"));
}