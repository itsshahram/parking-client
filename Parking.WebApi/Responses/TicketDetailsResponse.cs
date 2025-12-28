namespace Parking.WebApi.Responses;

public class TicketDetailsResponse
{
    public string TicketId { get; set; } = string.Empty;
    public string BarcodeId { get; set; } = string.Empty;
    public string EnLicensePlate { get; set; } = string.Empty;
    public string FaLicensePlate { get; set; } = string.Empty;
    public List<string> Images { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public string BarcodePrintType { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime InquiryDate { get; set; }
    public string TariffName { get; set; } = string.Empty;
}