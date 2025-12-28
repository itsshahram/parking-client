namespace Parking.WebApi.Application.Common.Exceptions;

public class CustomNotFoundException(string message) : Exception(message);