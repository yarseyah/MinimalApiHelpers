namespace MinimalApiHelpers.Exceptions;

public class InternalServerErrorException : Exception
{
    public InternalServerErrorException()
    {
    }

    public InternalServerErrorException(string errorMessage)
        : base(errorMessage)
    {
    }

    public InternalServerErrorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}