---
name: minimal-api-helpers
description: >
  Guides the creation and modification of ASP.NET Core Minimal API endpoints using the
  MinimalApiHelpers library. Use when adding new endpoints, endpoint groups, or
  request validation to any service in this solution.
---

# MinimalApiHelpers — Usage Guide

This library provides a structured, filter-based pattern for building ASP.NET Core Minimal API endpoints. It is used by all three backend services (`Fixtures.Service`, `Teams.Service`, `Statistics.Service`).

## Packages

The library ships as two NuGet packages:

| Package | Purpose |
|---|---|
| [`MinimalApiHelpers`](https://www.nuget.org/packages/MinimalApiHelpers) | Core helpers: `IEndpoint`, `MapEndpoint`, group builders, timing/exception filters |
| [`MinimalApiHelpers.FluentValidation`](https://www.nuget.org/packages/MinimalApiHelpers.FluentValidation) | FluentValidation integration: `WithRequestValidation`, `WithRouteParameterValidation`, `RequestValidationFilter`, `ValidationExceptionFilter` |

Add only the base package if you do not need validation. Add both when endpoints require FluentValidation-backed request validation.

```xml
<PackageReference Include="MinimalApiHelpers" />
<!-- optional — only needed when using WithRequestValidation / WithRouteParameterValidation -->
<PackageReference Include="MinimalApiHelpers.FluentValidation" />
```

These packages are intentionally minimal and do not include any dependencies beyond `Microsoft.AspNetCore.*` and `FluentValidation` (for the FluentValidation package). This allows you to use them in any service without worrying about unwanted dependencies or version conflicts.

Always check NuGet for the latest version and update the reference in your project file accordingly.

---

## Service Registration (`Program.cs`)

Two things must be wired up in `Program.cs`:

### 1. Register FluentValidation validators

Validators are resolved from DI by `RequestValidationFilter<TRequest>`. Register all validator assemblies — typically the API host assembly and any separate logic/query assembly:

```csharp
// Register validators from the API host assembly and any logic/query assemblies
builder.Services.AddValidatorsFromAssemblies([
    typeof(GetFixture.Validator).Assembly,   // endpoint validators (Apis/)
    typeof(GetFixtureQueryValidator).Assembly // logic-layer validators
]);
```

When a service encapsulates its own registration, this can be wrapped in a local extension method:

```csharp
// Teams.Service/Extensions/ServiceCollection/AddApiEndpointServices.cs
internal static IServiceCollection AddApiEndpointServices(this IServiceCollection services)
{
    services.AddValidatorsFromAssemblies([
        typeof(GetTeamByCode.Validator).Assembly,
        typeof(SomeLogicValidator).Assembly
    ]);
    // ...other endpoint-related registrations
    return services;
}
```

Called in `Program.cs` as:

```csharp
// The 'MapApiEndpoints' later will expect validators to have been registered.
builder.Services.AddApiEndpointServices();
```

### 2. Map endpoints

After `builder.Build()`, call the `MapApiEndpoints` extension to register all routes:

```csharp
var app = builder.Build();

app.UseHttpsRedirection();
app.MapApiEndpoints(); // registers all endpoints defined in Apis/
```

---

## Endpoint Folder & Namespace Structure

Each service organises its endpoints under an `Apis/` folder. Sub-folders represent logical resource groups and map directly to namespaces:

```
MyService/
└── Apis/
    ├── EndpointRouteBuilderExtensions.cs   ← registers all groups
    ├── Fixtures/
    │   ├── GetFixture.cs
    │   ├── ListFixtures.cs
    │   ├── ListFixturesByTeam.cs
    │   ├── DeleteFixture.cs
    │   └── CreateFixtures/
    │       └── Endpoint.cs                 ← complex endpoints get their own folder
    ├── Seasons/
    │   ├── GetSeason.cs
    │   ├── ListSeasons.cs
    │   └── PointAdjustments/
    │       ├── GetPointAdjustment.cs
    │       └── AddPointAdjustment.cs
    └── Teams/
        ├── GetTeamByCode.cs
        └── ListTeams.cs
```

**Namespace convention:** `{Service}.Apis.{Resource}`, e.g. `Fixtures.Service.Apis.Fixtures`.

**When to use a sub-folder per endpoint:** When an endpoint has enough supporting types (request records, validators, helpers) to justify isolation, create a folder named after the endpoint and put `Endpoint.cs` inside it. Otherwise, keep the endpoint as a single file named after the operation.

**`EndpointRouteBuilderExtensions.cs`** is the single wiring point — it calls `MapPublicGroup` / `MapProtectedGroup` and chains `MapEndpoint<T>()` for every endpoint. Add new endpoints here when creating them:

```csharp
internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        // Public (read) endpoints
        builder.MapPublicGroup("/fixtures").WithTags("Fixtures")
            .AddValidationFilters() // when endpoints in this group use request/route validation
            .MapEndpoint<GetFixture>()
            .MapEndpoint<ListFixtures>()
            .MapEndpoint<ListFixturesByTeam>();

        // Protected (write) endpoints
        builder.MapProtectedGroup("/fixtures").WithTags("Fixtures")
            .AddValidationFilters() // when endpoints in this group use request/route validation
            .MapEndpoint<CreateFixtures>()
            .MapEndpoint<DeleteFixture>();

        return builder;
    }
}
```

---

## Core Concept: `IEndpoint`

Every endpoint is a class that implements `IEndpoint`:

```csharp
public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder builder);
}
```

The static abstract `Map` method is the entry point for route registration. Implement it to attach a route and its filters to the builder.

## Endpoint Anatomy

Every endpoint follows this structure:

```csharp
[UsedImplicitly]
public sealed class GetFixture : IEndpoint
{
    // 1. Route registration + filter chain
    public static void Map(IEndpointRouteBuilder builder) => builder
        .MapGet("/{id}", Handle)
        .WithName(nameof(GetFixture))
        .WithSummary("Get a fixture by ID.")
        .WithRequestValidation<Request>()
        .WithEnsureEntityExists<Request, FixtureLookup>(
            (context, r, lookup, ct) => lookup.CheckExists(context, r.Id, ct))
        .Produces<Ok<FixtureDto>>();

    // 2. Handler — always async ValueTask, injects dependencies as parameters
    private static async ValueTask<Ok<FixtureDto>> Handle(
        ILogger<GetFixture> logger,
        ISender mediator,
        [AsParameters] Request request)
    {
        // ...
    }

    // 3. Strongly-typed request record — nested in the endpoint class
    [UsedImplicitly]
    public sealed record Request(string Id, bool? IncludeMetadata);

    // 4. FluentValidation validator — nested in the endpoint class
    [UsedImplicitly]
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
```

**Rules:**
- The handler must be `private static` and use `ValueTask<T>` (or `async ValueTask<T>`).
- Query string / route parameters are bound with `[AsParameters] Request request`.
- Inject services (logger, mediator, etc.) as leading handler parameters before `[AsParameters]`.
- **Optional based upon preferences and whether AOT code trimming is enabled**.  Because the `Request` and `Validator` types are only referenced via reflection in filters, decorate the endpoint class with something like `[UsedImplicitly]` (from `JetBrains.Annotations`) to prevent trimming and removal of warnings about these being unused.

---

## Registering Endpoints

### 1. Group registration (`EndpointRouteBuilderExtensions`)

Each service has an `internal static class EndpointRouteBuilderExtensions` that orchestrates all endpoints. Add new endpoints here:

```csharp
internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPublicEndpoints();
        builder.MapRestrictedEndpoints();
        return builder;
    }

    private static void MapPublicEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPublicGroup("/fixtures")
            .AddValidationFilters()
            .MapEndpoint<GetFixture>()
            .MapEndpoint<ListFixtures>();
    }

    private static void MapRestrictedEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapProtectedGroup("/fixtures")
            .AddValidationFilters()
            .MapEndpoint<CreateFixture>()
            .MapEndpoint<DeleteFixture>();
    }
}
```

This is called once in `Program.cs` with `app.MapApiEndpoints()`.

### 2. `MapPublicGroup` vs `MapProtectedGroup`

| Method | Auth | Use for |
|---|---|---|
| `MapPublicGroup(prefix)` | Anonymous | Read-only / public endpoints |
| `MapProtectedGroup(prefix)` | No auth policy applied automatically | Write / admin endpoints where auth is added explicitly |

Both automatically apply `AddDefaultFilters()` which attaches:
- `TimingFilter`, 
- `InternalServerErrorExceptionFilter`, 
- `EntityExistsVerificationFailureExceptionFilter`, 
- `InternalExceptionsFilter`.

To include validation exception handling for groups that use `WithRequestValidation(...)` or `WithRouteParameterValidation(...)`, use one of these patterns:

1. Add `.AddValidationFilters()` after `MapPublicGroup(...)` / `MapProtectedGroup(...)`.
2. Use `MapPublicGroupWithValidation(...)` / `MapProtectedGroupWithValidation(...)` convenience helpers.

### 3. `MapEndpoint<TEndpoint>()`

Chains onto a `RouteGroupBuilder` and calls `TEndpoint.Map(builder)`:

```csharp
builder.MapPublicGroup("/teams")
    .MapEndpoint<GetTeamByCode>()
    .MapEndpoint<ListTeams>();
```

---

## Route Handler Builder Extensions

These are fluent extensions applied to the result of `.MapGet(...)` / `.MapPost(...)` etc.

### `WithRequestValidation<TRequest>()` *(requires `MinimalApiHelpers.FluentValidation`)*

Registers a `RequestValidationFilter<TRequest>` that resolves the `IValidator<TRequest>` from DI and validates the request before the handler runs. If validation fails, the filter throws a `ValidationException` which the `ValidationExceptionFilter` converts to a `400 Bad Request` ProblemDetails response. Automatically marks the endpoint as producing `ValidationProblem`.

```csharp
builder.MapGet("/", Handle)
    .WithRequestValidation<Request>();
```

Requires:
1. A `Validator : AbstractValidator<Request>` nested in the endpoint class (or a separate partial class file).
2. Validators registered via `AddValidatorsFromAssemblies(...)` in `Program.cs` — the filter resolves `IValidator<TRequest>` from DI at runtime.
3. The containing route group must include `ValidationExceptionFilter` via `.AddValidationFilters()` or the validation-specific group helpers.

### `WithEnsureEntityExists<TRequest, TExistsLookup>(...)`

Resolves `TExistsLookup` from DI and calls the provided delegate before the handler. Returns `404 Not Found` if the delegate returns `false`. Automatically marks the endpoint as producing `404`.

```csharp
.WithEnsureEntityExists<Request, FixtureLookup>(
    (context, request, lookup, ct) => lookup.CheckFixtureExists(context, FixtureId.Parse(request.Id)))
```

- `TExistsLookup` must be registered in DI.
- The delegate signature is `Func<HttpContext, TRequest, TExistsLookup, CancellationToken, ValueTask<bool>>`.
- Can compose multiple existence checks:
  ```csharp
  .WithEnsureEntityExists<Request, TeamExistsLookup>(
      async (_, r, lookup, ct) =>
          await lookup.TeamCodeExists(r.TeamCode, ct) &&
          await lookup.TeamImageExists(r.TeamCode, r.ImageType, ct))
  ```

### `WithRouteParameterValidation<TParam>()` *(requires `MinimalApiHelpers.FluentValidation`)*

For validating a single route segment type (e.g., a strongly-typed ID parsed from the URL). Applies `RouteParameterValidationFilter<TParam>` and marks the endpoint as producing `ValidationProblem`.

```csharp
builder.MapGet("/{id}", Handle)
    .WithRouteParameterValidation<FixtureId>();
```

### `WithRequestedMediaTypeValidation(string[] supportedMediaTypes, string? defaultMediaType)`

Validates the `Accept` header against `supportedMediaTypes`. Returns `406 Not Acceptable` if no match. Stores the resolved media type in `HttpContext.Items["RequestedMediaType"]` for use in the handler or validator.

```csharp
builder.MapGet("/", Handle)
    .WithRequestedMediaTypeValidation(
        ["application/json", "text/csv"],
        defaultMediaType: "application/json")
    .WithRequestValidation<Request>();
```

> **Important:** Place `WithRequestedMediaTypeValidation` *before* `WithRequestValidation` so the resolved media type is available in the validator via `IHttpContextAccessor`.

Reading the resolved type in the handler via `HttpContext`:

```csharp
var mediaType = context.Items["RequestedMediaType"] as string;
return mediaType switch
{
    "application/json" => await HandleJson(...),
    "text/csv"         => await HandleCsv(...),
    _ => throw new InternalServerErrorException($"Unsupported media type: {mediaType}"),
};
```

Reading the resolved type in a validator (requires `IHttpContextAccessor`):

```csharp
public Validator(IHttpContextAccessor contextAccessor)
{
    var mediaType = contextAccessor.HttpContext?.Items["RequestedMediaType"] as string ?? string.Empty;
    When(_ => mediaType == "application/json", () => { /* json-specific rules */ });
}
```

---

## Exceptions

Throw these inside handlers to trigger the corresponding built-in exception filter:

| Exception | Filter | HTTP Response |
|---|---|---|
| `InternalServerErrorException` | `InternalServerErrorExceptionFilter` | 500 Internal Server Error |
| `BadRequestException` | (caught by `InternalExceptionsFilter`) | 400 Bad Request |
| `FluentValidation.ValidationException` | `ValidationExceptionFilter` | 400 / 404 (error-code-dependent) |

```csharp
// In a handler when an unexpected condition arises:
throw new InternalServerErrorException("Unexpected image type returned from query.");
```

---

## `ProblemDetailsHelper`

Use to construct consistent `ProblemDetails` responses manually (e.g., in complex handlers):

```csharp
var problem = ProblemDetailsHelper.Create(
    context,
    title: "Bad Request",
    statusCode: 400,
    errors: new Dictionary<string, string[]>
    {
        ["TeamCode"] = ["Team code must be 3 characters."]
    });
```

Chain `.AddError(field, messages)` to append additional fields:

```csharp
problem.AddError("SeasonId", "Season does not exist.");
```

---

## Filters Applied Automatically

The following filters are registered on all groups via `AddDefaultFilters()` — you do not need to add them manually:

| Filter | Behaviour |
|---|---|
| `TimingFilter` | Appends `X-Elapsed-Milliseconds` to every response |
| `InternalServerErrorExceptionFilter` | Converts `InternalServerErrorException` to 500 ProblemDetails |
| `EntityExistsVerificationFailureExceptionFilter` | Handles entity-lookup failures from `WithEnsureEntityExists` |
| `InternalExceptionsFilter` | Catch-all for unhandled exceptions |

Validation-enabled groups additionally register:

| Filter | Behaviour |
|---|---|
| `ValidationExceptionFilter` | Converts `FluentValidation.ValidationException` to 400 ProblemDetails (or 404 for specific error codes) |

`MapProtectedGroup(...)` does not currently enforce authorization on its own. Apply authorization policies explicitly where required.

---

## Checklist: Adding a New Endpoint

1. Create a class implementing `IEndpoint` in the appropriate `Apis/` subfolder.
2. Decorate the class and nested types with `[UsedImplicitly]` if `JetBrains.Annotations` is available and used elsewhere in the project/solution.
3. Define a nested `Request` record for query string / route parameters.
4. Define a nested `Validator : AbstractValidator<Request>` with FluentValidation rules.
5. Write a `private static (async) ValueTask<T> Handle(...)` method.
6. In `Map`, chain `.WithRequestValidation<Request>()` and optionally `.WithEnsureEntityExists<>()`.
7. Ensure the containing route group adds validation exception handling via `.AddValidationFilters()` or `MapPublicGroupWithValidation(...)` / `MapProtectedGroupWithValidation(...)`.
8. Register the endpoint in the service's `EndpointRouteBuilderExtensions` under the appropriate group.
9. Ensure validators are picked up by `AddValidatorsFromAssemblies(...)` in `Program.cs` (they are, as long as the assembly is already included).
