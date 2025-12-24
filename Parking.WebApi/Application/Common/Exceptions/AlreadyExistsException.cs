namespace Parking.WebApi.Application.Common.Exceptions;

public class AlreadyExistsException(string message) : Exception(message);