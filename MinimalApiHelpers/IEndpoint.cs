namespace MinimalApiHelpers;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder builder);
}

