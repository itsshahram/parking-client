namespace Parking.WebApi.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException()
        : base("یک یا چند خطای اعتبارسنجی رخ داده است.")
    {
        Errors = new List<string>();
    }

    public ValidationException(IEnumerable<string> errors)
        : this()
    {
        Errors = errors.ToList();
    }
}
