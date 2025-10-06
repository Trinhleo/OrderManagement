namespace OrderManagement.Api.Exceptions;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new List<ValidationFailure>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<ValidationFailure>();
    }
}

public class ValidationFailure
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public ValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }
}