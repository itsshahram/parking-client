namespace Parking.WebApi.Application.Common.Exceptions;

public class LicensePlateSeizedException(string message) : Exception(message);