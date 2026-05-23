namespace MinimalApiHelpers;

public interface IEndpoint
{
    /// <summary>Register this endpoint's route(s) on the builder and return the primary route handler.</summary>
    static abstract RouteHandlerBuilder Map(IEndpointRouteBuilder builder);

    /// <summary>
    /// Optional named authorization policy to apply to this endpoint's route handler.
    /// When non-null, <c>MapEndpoint</c> will chain <c>.RequireAuthorization(policy)</c>
    /// on the returned handler.
    /// </summary>
    static virtual string? AuthorizationPolicy => null;
}

