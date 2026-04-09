# MinimalApiHelpers

MinimalApiHelpers packages a consistent pattern for ASP.NET Core Minimal APIs: endpoint classes that implement `IEndpoint`, route-group helpers for shared filters, and optional FluentValidation integration for request and route validation.

## Packages

### Core Package

[![NuGet](https://img.shields.io/nuget/v/MinimalApiHelpers.svg)](https://www.nuget.org/packages/MinimalApiHelpers)

Core features:

- `IEndpoint` and `MapEndpoint<TEndpoint>()`
- `MapPublicGroup(...)` and `MapProtectedGroup(...)`
- default timing and exception filters
- route-handler helpers such as `WithEnsureEntityExists(...)` and `WithRequestedMediaTypeValidation(...)`

### FluentValidation Package

[![NuGet](https://img.shields.io/nuget/v/MinimalApiHelpers.FluentValidation.svg)](https://www.nuget.org/packages/MinimalApiHelpers.FluentValidation)

Validation features:

- `WithRequestValidation<TRequest>()`
- `WithRouteParameterValidation<TParam>()`
- `AddValidationFilters()` for route groups
- `MapPublicGroupWithValidation(...)` and `MapProtectedGroupWithValidation(...)` convenience helpers

## Installation

```bash
# Core package
dotnet add package MinimalApiHelpers

# Add this when you need FluentValidation-backed request or route validation
dotnet add package MinimalApiHelpers.FluentValidation
```

## Recommended Structure

Organize endpoints under an `Apis/` folder, and keep all route registration behind a single extension method.

```text
MyService/
|- Apis/
|  |- EndpointRouteBuilderExtensions.cs
|  |- Widgets/
|  |  |- GetWidget.cs
|  |  |- CreateWidget.cs
|  |- Health/
|     |- GetHealth.cs
|- Program.cs
```

In `Program.cs`, register validators, build the app, and call one mapping method:

```csharp
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapApiEndpoints();

await app.RunAsync();
```

In `Apis/EndpointRouteBuilderExtensions.cs`, group routes and chain `MapEndpoint<TEndpoint>()`:

```csharp
namespace MyService.Apis;

internal static class EndpointRouteBuilderExtensions
{
   public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
   {
      builder.MapPublicGroup("/widgets")
         .WithTags("Widgets")
         .AddValidationFilters()
         .MapEndpoint<GetWidget>();

      builder.MapProtectedGroup("/widgets")
         .WithTags("Widgets")
         .AddValidationFilters()
         .MapEndpoint<CreateWidget>();

      return builder;
   }
}
```

`AddValidationFilters()` is required when a group contains endpoints that use `WithRequestValidation(...)` or `WithRouteParameterValidation(...)`. If you prefer a convenience method, use `MapPublicGroupWithValidation(...)` or `MapProtectedGroupWithValidation(...)` instead.

## Endpoint Example

Each endpoint is a class that implements `IEndpoint`, owns its request contract, and optionally owns its validator.

```csharp
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MyService.Apis.Widgets;

[UsedImplicitly]
public sealed class GetWidget : IEndpoint
{
   public static void Map(IEndpointRouteBuilder builder)
      => builder.MapGet("/{id}", Handle)
         .WithName(nameof(GetWidget))
         .WithSummary("Get a widget by id.")
         .WithRequestValidation<Request>()
         .Produces<Ok<string>>();

   private static ValueTask<Ok<string>> Handle([AsParameters] Request request)
      => ValueTask.FromResult(TypedResults.Ok($"widget:{request.Id}"));

   [UsedImplicitly]
   public sealed record Request(string? Id);

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

## Validation and Filters

- `MapPublicGroup(...)` and `MapProtectedGroup(...)` automatically add timing and exception filters.
- `WithRequestValidation<TRequest>()` validates a bound request object using `IValidator<TRequest>` from DI.
- `WithRouteParameterValidation<TParam>()` validates a single bound route parameter type.
- `WithEnsureEntityExists<TRequest, TLookup>(...)` short-circuits with `404 Not Found` when a lookup fails.
- `WithRequestedMediaTypeValidation(...)` validates the `Accept` header and stores the selected media type in `HttpContext.Items["RequestedMediaType"]`.

## Demo and Docs

- The sample app in `Demo/` follows the recommended endpoint structure.
- Detailed guidance is available in [docs/ENDPOINT_PATTERNS.md](docs/ENDPOINT_PATTERNS.md).
- Packaging and release notes remain in [docs/NUGET_SETUP.md](docs/NUGET_SETUP.md) and [docs/MINVER_SETUP.md](docs/MINVER_SETUP.md).

## Contributing

Contributions are welcome. Keep examples and documentation aligned with the `IEndpoint` pattern used by the demo.

## License

This project is licensed under the MIT License. See `LICENSE` for details.
