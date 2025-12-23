namespace Parking.WebApi.Application.Common.Exceptions;

public class ValidationException() : Exception("یک یا چند خطای اعتبارسنجی رخ داده است.")
{
    public List<string> Errors { get; } = [];

    public ValidationException(IEnumerable<string> errors)
        : this()
    {
        Errors = errors.ToList();
    }
}
