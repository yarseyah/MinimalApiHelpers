namespace MinimalApiHelpers.Filters;

/// <summary>
/// Endpoint filter that resolves whether the current user holds the specified role
/// and stores the result in <c>HttpContext.Items</c> before the handler executes.
/// This is for <em>data-shaping</em>, not access gating — the handler uses the pre-computed
/// flag to decide what data to return, rather than calling <c>IsInRole()</c> directly.
/// </summary>
internal sealed class UserRoleFilter : IEndpointFilter
{
    private readonly string _roleName;
    private readonly string _itemKey;

    public UserRoleFilter(string roleName)
    {
        _roleName = roleName;
        _itemKey = $"Role:{roleName}";
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        context.HttpContext.Items[_itemKey] =
            user.Identity?.IsAuthenticated == true && user.IsInRole(_roleName);

        return await next(context);
    }
}