namespace MinimalApiHelpers.Filters;

/// <summary>
/// Endpoint filter that resolves whether the current user holds the specified role
/// and stores the result in <c>HttpContext.Items</c> before the handler executes.
/// This is for <em>data-shaping</em>, not access gating — the handler uses the pre-computed
/// flag to decide what data to return, rather than calling <c>IsInRole()</c> directly.
/// </summary>
internal sealed class UserRoleFilter(string roleName) : IEndpointFilter
{
    private readonly string itemKey = $"Role:{roleName}";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        context.HttpContext.Items[itemKey] = user.Identity?.IsAuthenticated == true
                                             && user.IsInRole(roleName);

        return await next(context);
    }
}