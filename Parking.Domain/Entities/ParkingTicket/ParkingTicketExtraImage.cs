using System.ComponentModel.DataAnnotations;



namespace Parking.Domain.Entities.ParkingTicket;

public class ParkingTicketExtraImage
{
    [Key]
    public long Id { get; set; }
    public Guid TicketId { get; set; }
    public string? FaName { get; set; }
    public string? Image { get; set; }
    public string? GateName { get; set; }
    public bool ShowInPage { get; set; }
    public DateTime CreateDateTime { get; set; }
}
