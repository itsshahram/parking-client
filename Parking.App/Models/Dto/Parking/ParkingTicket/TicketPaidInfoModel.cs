
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class TicketPaidInfoModel
{
    public Guid TicketId { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaidCreditCard { get; set; }
    [MaxLength(150)]
    public string? PaidType { get; set; }
    public string? RefId { get; set; }
    public string? PaidDate { get; set; }
    public string? MerchantNumber { get; set; }
    public string? RRN { get; set; }
    public string? TraceNo { get; set; }
    public string? ExitGate { get; set; }
    public bool IsMissingCard { get; set; }
    public long? CardUid { get; set; }
}
