namespace MinimalApiHelpers.Extensions.HttpContextExtensions;

/// <summary>Extensions for reading pre-computed role flags from <c>HttpContext.Items</c>.</summary>
public static class HttpContextUserRoleExtensions
{
    /// <summary>
    /// Returns <c>true</c> if a <c>WithUserRole(roleName)</c> filter determined that
    /// the current user holds the specified role. Returns <c>false</c> if the user
    /// does not hold the role, and <c>null</c> if no <c>WithUserRole</c> filter was
    /// registered for this role (i.e. the flag was never set).
    /// </summary>
    public static bool? GetUserHasRole(this Microsoft.AspNetCore.Http.HttpContext httpContext, string roleName)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        return httpContext.Items[$"Role:{roleName}"] as bool?;
    }
}