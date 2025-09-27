using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.Domain.Entities.ParkingTicket;

public class TicketQueueItem
{
    [Key]
    public Guid Id { get; set; }

    public int TicketDescriptionItemId { get; set; }

    [ForeignKey(nameof(TicketDescriptionItemId))]
    public TicketDescriptionItem? TicketDescriptionItem { get; set; }

    public int QueueNumber { get; set; }

    public DateTime AssignedDate { get; set; }

    public Guid ParkingTicketId { get; set; }
    public int ResetVersion { get; set; }

    [ForeignKey(nameof(ParkingTicketId))]
    public ParkingTicket? ParkingTicket { get; set; }
}
