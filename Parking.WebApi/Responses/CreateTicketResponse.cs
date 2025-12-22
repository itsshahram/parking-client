namespace Parking.WebApi.Responses;

public class CreateTicketResponse
{
    public Guid TicketId { get; set; }
    public string BarcodeId { get; set; } = string.Empty;
}