namespace Demo;

[UsedImplicitly]
public sealed class ValidatedPingEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/ping", Handler)
            .WithRequestValidation<Request>();

    private static Ok<string> Handler(
        [AsParameters] Request request) 
        => TypedResults.Ok($"Pong: {request.Id}");

    [UsedImplicitly]
    public sealed record Request(
        string? Id
    );

    [UsedImplicitly]
    public sealed class RequestValidator : AbstractValidator<Request>
    {
        private static readonly string[] PermittedIds = ["foo", "bar"];

        public RequestValidator(ILogger<RequestValidator> logger)
        {
            RuleLevelCascadeMode = ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .NotNull()
                .NotEmpty()
                .Must(id => PermittedIds.Contains(id, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Invalid Id, only {Listify(PermittedIds)} are permitted");
        }

        private static string Listify(string[] values)
        {
            return values.Length switch
            {
                <=0 => throw new InvalidOperationException("Cannot listify an empty array"),
                1 => Quote(values[0]),
                >=2 => Quoted(values),
            };

            string Quoted(string[] input)
            {
                var length = input.Length;
                var parts = input
                    .Select(
                        (item, index) =>
                            index + 1 == length
                                ? $"and {Quote(item)}"
                                : $"{Quote(item)}, ");
                return string.Concat(parts);
            }

            string Quote(string input) => $"'{input}'";
        }
    }
}