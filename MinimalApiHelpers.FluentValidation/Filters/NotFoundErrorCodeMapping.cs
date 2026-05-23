namespace MinimalApiHelpers.FluentValidation.Filters;

/// <summary>
/// A mapping of FluentValidation error codes that should result in a 404 Not Found response
/// instead of the default 400 Bad Request. Register this as a singleton and add error codes
/// to convert specific validation failures to NotFound responses.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// var mapping = new NotFoundErrorCodeMapping();
/// mapping.Add("team-not-found");
/// builder.Services.AddSingleton(mapping);
/// </code>
/// If no <see cref="NotFoundErrorCodeMapping"/> is registered, all validation errors remain 400 Bad Request.
/// </remarks>
[PublicAPI]
public class NotFoundErrorCodeMapping : HashSet<string>;
