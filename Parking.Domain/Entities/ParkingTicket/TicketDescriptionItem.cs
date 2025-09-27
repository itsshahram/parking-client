using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.Entities.ParkingTicket;

public class TicketDescriptionItem
{
    [Key]
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsQueueEnabled { get; set; } = false;
    public bool IsDeleted { get; set; }
    public int CurrentResetVersion { get; set; }
}
