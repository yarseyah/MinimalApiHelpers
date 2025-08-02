namespace MinimalApiHelpers.Exceptions;

public class LookedUpEntityWasNotActuallyFoundException : Exception
{
    public LookedUpEntityWasNotActuallyFoundException()
    {
    }

    public LookedUpEntityWasNotActuallyFoundException(string message)
        : base(message)
    {
    }

    public LookedUpEntityWasNotActuallyFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public required Type RequestType { get; init; }

    public required string EntityName { get; init; }

    public required string EntityValue { get; init; }
}