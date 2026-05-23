namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Apply a named authorization policy to the route group.
    /// Fluent shorthand for <c>.RequireAuthorization(policyName)</c>.
    /// </summary>
    public static RouteGroupBuilder WithPolicy(
        this RouteGroupBuilder builder,
        string policyName)
        => builder.RequireAuthorization(policyName);
}