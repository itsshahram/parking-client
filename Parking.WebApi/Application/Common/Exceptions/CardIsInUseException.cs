namespace Parking.WebApi.Application.Common.Exceptions;

public class CardIsInUseException(string message) : Exception(message);