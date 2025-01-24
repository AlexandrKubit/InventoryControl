namespace Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message)
    {
        Message = message;
    }

    public override string Message { get; }
}
