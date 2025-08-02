# MinimalApiHelpers

A collection of helpers and extensions for ASP.NET Core Minimal APIs that simplify common tasks like timing, exception handling, and validation.

## Packages

### MinimalApiHelpers
[![NuGet](https://img.shields.io/nuget/v/MinimalApiHelpers.svg)](https://www.nuget.org/packages/MinimalApiHelpers)

Core helpers for ASP.NET Core Minimal APIs including:
- `TimingFilter` - Adds response timing headers
- Exception handling filters
- Endpoint extension methods

### MinimalApiHelpers.FluentValidation
[![NuGet](https://img.shields.io/nuget/v/MinimalApiHelpers.FluentValidation.svg)](https://www.nuget.org/packages/MinimalApiHelpers.FluentValidation)

FluentValidation integration for MinimalApiHelpers including:
- Request validation filters
- Route parameter validation
- Validation exception handling

## Installation

```bash
# Core package
dotnet add package MinimalApiHelpers

# FluentValidation extensions
dotnet add package MinimalApiHelpers.FluentValidation
```

## Usage

### Basic Setup

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Add timing filter to all endpoints
app.MapGet("/api/example", () => "Hello World!")
   .AddFilter<TimingFilter>();

app.Run();
```

### With FluentValidation

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Add validation filters
app.MapPost("/api/example", (ExampleRequest request) => "Success")
   .WithRequestValidation<ExampleRequest>();

app.Run();
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
# Test change
