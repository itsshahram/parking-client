namespace Parking.WebApi.Application.Common.Exceptions;

public class NotAllowedException(string message) : Exception(message);