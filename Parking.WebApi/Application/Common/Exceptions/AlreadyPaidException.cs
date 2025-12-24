namespace Parking.WebApi.Application.Common.Exceptions;

public class AlreadyPaidException(string message) : Exception(message);