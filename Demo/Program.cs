var builder = WebApplication.CreateBuilder(args);

// Allow the Dependency Injection to know about all `AbstractValidator<T>`
builder.Services.AddValidatorsFromAssemblyContaining<ValidatedPingEndpoint>();

var app = builder.Build();

app.MapPublicGroupWithValidation("/validated")
    .MapEndpoint<ValidatedPingEndpoint>()
    .MapEndpoint<PongEndpoint>();

app.MapPublicGroup("/unvalidated")
    .MapEndpoint<UnvalidatedPingEndpoint>()
    .MapEndpoint<PongEndpoint>();

await app.RunAsync();