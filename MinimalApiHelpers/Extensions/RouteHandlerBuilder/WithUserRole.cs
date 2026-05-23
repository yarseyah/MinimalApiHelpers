using MinimalApiHelpers.Filters;

namespace MinimalApiHelpers.Extensions.RouteHandlerBuilder;

/// <summary>Data-shaping extensions — pre-computing role presence for handler consumption.</summary>
public static class WithUserRoleExtensions
{
    /// <summary>
    /// Add a filter that resolves whether the current user holds <paramref name="roleName"/>
    /// and stores the result in <c>HttpContext.Items["Role:{roleName}"]</c>.
    /// The handler reads it via <c>httpContext.GetUserHasRole(roleName)</c>.
    /// This is non-gating — it does not return 403.
    /// </summary>
    public static Microsoft.AspNetCore.Builder.RouteHandlerBuilder WithUserRole(
        this Microsoft.AspNetCore.Builder.RouteHandlerBuilder builder,
        string roleName)
        => builder.AddEndpointFilter(new UserRoleFilter(roleName));
}