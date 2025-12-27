namespace Parking.WebApi.Requests;

public class PaymentRequest
{
    public string PaidType { get; set; } = string.Empty;
    public Guid TicketId { get; set; }
    public List<string>? Base64Images { get; set; }
    public long Amount { get; set; }
    public string ExitGate { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public DateTime PaidDate { get; set; }
    public string PaidCreditCard { get; set; } = string.Empty;
    public string MerchantNumber { get; set; } = string.Empty;
    public string Rrn { get; set; } = string.Empty;
    public string TraceNo { get; set; } = string.Empty;
    public string RefId { get; set; } = string.Empty;
}