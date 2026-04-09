# Endpoint Patterns

This guide documents the recommended way to use MinimalApiHelpers in an ASP.NET Core Minimal API service.

## Packages

Use `MinimalApiHelpers` for the core endpoint model and add `MinimalApiHelpers.FluentValidation` when you need request or route validation.

```xml
<ItemGroup>
  <PackageReference Include="MinimalApiHelpers" />
  <PackageReference Include="MinimalApiHelpers.FluentValidation" />
</ItemGroup>
```

## Program Setup

Register validators before building the app, then map all endpoints from one place.

```csharp
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapApiEndpoints();

await app.RunAsync();
```

## Folder Layout

Keep endpoints under `Apis/` and use logical subfolders for resource groups.

```text
MyService/
|- Apis/
|  |- EndpointRouteBuilderExtensions.cs
|  |- Orders/
|  |  |- GetOrder.cs
|  |  |- CreateOrder.cs
|  |- Health/
|     |- GetHealth.cs
|- Program.cs
```

Complex endpoints can use their own folder with an `Endpoint.cs` file when they need extra request models, helpers, or validators.

## Single Wiring Point

Expose one extension method that registers all groups and endpoints.

```csharp
namespace MyService.Apis;

internal static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPublicGroup("/orders")
            .WithTags("Orders")
            .AddValidationFilters()
            .MapEndpoint<GetOrder>();

        builder.MapProtectedGroup("/orders")
            .WithTags("Orders")
            .AddValidationFilters()
            .MapEndpoint<CreateOrder>();

        return builder;
    }
}
```

`MapPublicGroup(...)` and `MapProtectedGroup(...)` add the core timing and exception filters.

If a group includes endpoints that use validation, add one of these:

- `.AddValidationFilters()`
- `.MapPublicGroupWithValidation(...)`
- `.MapProtectedGroupWithValidation(...)`

## Endpoint Anatomy

Every endpoint implements `IEndpoint` and exposes a static `Map` method.

```csharp
public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder builder);
}
```

Recommended endpoint structure:

```csharp
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MyService.Apis.Orders;

[UsedImplicitly]
public sealed class GetOrder : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/{id}", Handle)
            .WithName(nameof(GetOrder))
            .WithSummary("Get an order by id.")
            .WithRequestValidation<Request>()
            .Produces<Ok<string>>()
            .ProducesValidationProblem();

    private static ValueTask<Ok<string>> Handle([AsParameters] Request request)
        => ValueTask.FromResult(TypedResults.Ok($"order:{request.Id}"));

    [UsedImplicitly]
    public sealed record Request(string? Id, bool IncludeLines = false);

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

## Handler Rules

- Use `private static ValueTask<T>` for handlers.
- Put injected services first, then the `[AsParameters]` request.
- Keep the request record nested inside the endpoint when it only belongs to that endpoint.
- Keep the validator nested with the request when it is endpoint-specific.
- Add `[UsedImplicitly]` when you want to avoid trimming or analyzer noise for reflection-driven types.

Example with injected dependencies:

```csharp
private static async ValueTask<Ok<OrderDto>> Handle(
    ILogger<GetOrder> logger,
    IOrderQueryService orders,
    [AsParameters] Request request)
{
    var order = await orders.GetAsync(request.Id!);
    logger.LogInformation("Fetched order {OrderId}", request.Id);
    return TypedResults.Ok(order);
}
```

## Route Handler Extensions

### `WithRequestValidation(...)`

Validates the bound request using `IValidator<TRequest>` from DI.

Requirements:

- register validators with `AddValidatorsFromAssemblyContaining<...>()` or `AddValidatorsFromAssemblies(...)`
- ensure the containing group has validation exception filters through `AddValidationFilters()` or the validation group helpers

### `WithRouteParameterValidation(...)`

Validates a single bound route value, typically a strongly-typed ID.

```csharp
builder.MapGet("/{id}", Handle)
    .WithRouteParameterValidation<OrderId>();
```

### WithEnsureEntityExists<TRequest, TLookup>()

Short-circuits with `404 Not Found` when a lookup indicates the entity is missing.

```csharp
builder.MapGet("/{id}", Handle)
    .WithEnsureEntityExists<Request, OrderLookup>(
        (context, request, lookup, ct) => lookup.ExistsAsync(request.Id!, ct));
```

### WithRequestedMediaTypeValidation(...)

Validates the `Accept` header and stores the negotiated media type in `HttpContext.Items["RequestedMediaType"]`.

```csharp
builder.MapGet("/export", Handle)
    .WithRequestedMediaTypeValidation(
        ["application/json", "text/csv"],
        defaultMediaType: "application/json")
    .WithRequestValidation<Request>();
```

Place `WithRequestedMediaTypeValidation(...)` before `WithRequestValidation(...)` when the validator depends on the selected media type.

## Filters Added by Groups

`MapPublicGroup(...)` and `MapProtectedGroup(...)` include:

- `TimingFilter`
- `InternalServerErrorExceptionFilter`
- `EntityExistsVerificationFailureExceptionFilter`
- `InternalExceptionsFilter`

Validation groups add `ValidationExceptionFilter` on top of those core filters.

## Demo Reference

The `Demo/` project in this repository shows the pattern end-to-end:

- validator registration in `Program.cs`
- a single `MapApiEndpoints()` method
- endpoint classes grouped under `Apis/`
- validated and unvalidated routes using the same `IEndpoint` model
