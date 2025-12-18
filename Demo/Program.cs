var builder = WebApplication.CreateBuilder(args);

// Allow the Dependency Injection to know about all `AbstractValidator<T>`
builder.Services.AddValidatorsFromAssemblyContaining<PingEndpoint>();

var app = builder.Build();

app.MapPublicGroup("/api")
    .AddDefaultFiltersIncludingValidation()
    .MapEndpoint<PingEndpoint>()
    .MapEndpoint<PongEndpoint>();

await app.RunAsync();