namespace MinimalApiHelpers.Extensions.EndpointRouteBuilder;

using RouteGroupBuilder = Microsoft.AspNetCore.Routing.RouteGroupBuilder;

public static partial class EndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder AddDefaultFilters(this RouteGroupBuilder app)
        => app
            .AddEndpointFilter<TimingFilter>()
            .AddEndpointFilter<InternalServerErrorExceptionFilter>()
            .AddEndpointFilter<EntityExistsVerificationFailureExceptionFilter>()
            .AddEndpointFilter<InternalExceptionsFilter>();
}