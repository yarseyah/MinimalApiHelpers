namespace MinimalApiHelpers;

public class TimingFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var sw = Stopwatch.StartNew();
        var response = await next(context);
        context.HttpContext.Response.Headers.Append(
            "X-Elapsed-Milliseconds",
            sw.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
        return response;
    }
}