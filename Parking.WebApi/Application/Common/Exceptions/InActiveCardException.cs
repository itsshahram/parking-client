namespace Parking.WebApi.Application.Common.Exceptions;

public class InActiveCardException(string message) : Exception(message);