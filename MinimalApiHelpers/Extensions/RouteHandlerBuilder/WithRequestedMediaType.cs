namespace MinimalApiHelpers.Extensions.RouteHandlerBuilder;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using RouteHandlerBuilder = Microsoft.AspNetCore.Builder.RouteHandlerBuilder;

public static partial class RouteHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a request 'Accept' header validation filter to the route handler.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="supportedMediaTypes">Array of supported media types.</param>
    /// <param name="defaultMediaType">What media type to use when '*/*' specified.</param>
    /// <returns>
    /// A <see cref="RouteHandlerBuilder"/> that can be used to further customize the endpoint.
    /// </returns>
    public static RouteHandlerBuilder
        WithRequestedMediaTypeValidation(
            this RouteHandlerBuilder builder,
            string[] supportedMediaTypes,
            string? defaultMediaType = null)
    {
        // Ensure, if specified, that @default is a supported media type
        return defaultMediaType is null || supportedMediaTypes.Contains(defaultMediaType)
            ? AddMediaTypes(builder, supportedMediaTypes, defaultMediaType)
            : throw new ArgumentException(
                $"The default media type '{defaultMediaType}' is not in the list of supported media types.");
    }

    private static RouteHandlerBuilder AddMediaTypes(RouteHandlerBuilder builder, string[] supportedMediaTypes, string? defaultMediaType)
    {
        return builder.AddEndpointFilterFactory(
                (_, next) => async context =>
                {
                    var logger = GetLogger(context);

                    var requestedMediaTypes = GetRequestedMediaTypes(context.HttpContext.Request.Headers.Accept);

                    // In the case of nothing being specified, assume the default
                    if (requestedMediaTypes.Length == 0)
                    {
                        requestedMediaTypes = [new MediaTypeHeaderValue(defaultMediaType, 1.0)];
                    }

                    // Determine which of the supplied media types is the first match
                    // within the requested media types
                    var rankings = requestedMediaTypes
                        .Where(mt => mt.MatchesAllTypes || supportedMediaTypes.Contains(mt.MediaType.ToString()))
                        .ToArray();

                    // If rankings were found, log the results and store the top ranking media type
                    if (rankings.Length != 0)
                    {
                        foreach (var ranking in rankings.Select((r, i) => (r, i)))
                        {
                            logger.LogTrace(
                                "MediaType: {Type} with quality {Quality} ranked {Ranking}",
                                ranking.r.MediaType,
                                ranking.r.Quality ?? 1.0,
                                ranking.i);
                        }

                        // Store top ranking media type in the request context
                        context.HttpContext.Items["RequestedMediaType"] =
                            rankings[0].MatchesAllTypes
                            && defaultMediaType is not null
                                ? defaultMediaType
                                : rankings[0].MediaType.ToString();

                        return await next(context);
                    }

                    return GenerateProblemDetailsResult(
                        supportedMediaTypes,
                        logger,
                        context.HttpContext.Request.Headers.Accept,
                        context);
                })
            .ProducesValidationProblem(statusCode: StatusCodes.Status406NotAcceptable);
    }

    private static MediaTypeHeaderValue[] GetRequestedMediaTypes(StringValues acceptValues)
    {
        var segments = acceptValues
            .Where(s => s is not null)
            .Cast<string>()
            .SelectMany(header => header?.Split(',') ?? [])
            .ToList();

        var mediaTypes = MediaTypeHeaderValue
            .ParseList(segments)
            .OrderByDescending(mt => mt.Quality ?? 1.0);
        return [.. mediaTypes];
    }

    private static ProblemHttpResult GenerateProblemDetailsResult(
        string[] mediaTypes,
        ILogger logger,
        StringValues acceptHeader,
        EndpointFilterInvocationContext context)
    {
        logger.LogError(
            "Request is not acceptable. Accept header: '{AcceptHeader}' on request to '{Path}'",
            acceptHeader!,
            context.HttpContext.Request.Path);

        var acceptedTypes = string.Join(", ", mediaTypes);

        var errors = new Dictionary<string, string>
        {
            ["Accept-Header"] = $"Only the following media types are supported: {acceptedTypes}",
        };

        return TypedResults.Problem(
            new ProblemDetails
            {
                Title = "Not Acceptable",
                Status = StatusCodes.Status406NotAcceptable,
                Type = "https://rfc-editor.org/rfc/rfc7231#section-6.5.1",
                Instance = context.HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = context.HttpContext.TraceIdentifier,
                    ["errors"] = errors,
                },
            });
    }

    private static ILogger GetLogger(EndpointFilterInvocationContext context)
    {
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        return loggerFactory.CreateLogger("RequestedMediaTypeValidation");
    }
}