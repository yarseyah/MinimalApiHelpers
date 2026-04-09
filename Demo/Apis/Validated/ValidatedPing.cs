namespace Demo.Apis.Validated;

[UsedImplicitly]
public sealed class ValidatedPing : IEndpoint
{
    public static void Map(IEndpointRouteBuilder builder)
        => builder.MapGet("/ping", Handle)
            .WithName(nameof(ValidatedPing))
            .WithSummary("Return a validated pong response.")
            .WithRequestValidation<Request>()
            .Produces<Ok<string>>();

    private static ValueTask<Ok<string>> Handle([AsParameters] Request request)
        => ValueTask.FromResult(TypedResults.Ok($"Pong: {request.Id}"));

    [UsedImplicitly]
    public sealed record Request(string? Id);

    [UsedImplicitly]
    public sealed class Validator : AbstractValidator<Request>
    {
        private static readonly string[] PermittedIds = ["foo", "bar"];

        public Validator()
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
                <= 0 => throw new InvalidOperationException("Cannot listify an empty array"),
                1 => Quote(values[0]),
                _ => Quoted(values),
            };

            static string Quoted(string[] input)
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

            static string Quote(string input) => $"'{input}'";
        }
    }
}