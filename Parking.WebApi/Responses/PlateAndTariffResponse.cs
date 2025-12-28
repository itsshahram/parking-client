namespace Parking.WebApi.Responses;

public class PlateAndTariffResponse
{
    public string? EnLicencePlate { get; set; } = string.Empty;
    public string? FaLicensePlate { get; set; }
    public string? Tariff { get; set; } = string.Empty;
}