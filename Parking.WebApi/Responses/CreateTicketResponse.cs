namespace Parking.WebApi.Responses;

public class CreateTicketResponse
{
    public Guid TicketId { get; set; }
    public string BarcodeId { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string BarcodePrintType { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string TariffName { get; set; } = string.Empty;
}