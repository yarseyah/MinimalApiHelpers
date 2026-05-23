namespace MinimalApiHelpers.Extensions.RouteHandlerBuilder;

/// <summary>Access-gating extensions — returning 403 if the user lacks the required role(s).</summary>
public static class RequireRoleExtensions
{
    /// <summary>
    /// Require the authenticated user to hold at least one of the specified roles.
    /// Returns 403 if the requirement is not met.
    /// </summary>
    public static Microsoft.AspNetCore.Builder.RouteHandlerBuilder RequireRole(
        this Microsoft.AspNetCore.Builder.RouteHandlerBuilder builder,
        params string[] roles)
        => builder.RequireAuthorization(policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(roles);
        });
}