using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    [ForeignKey(nameof(ParkingTicketId))]
    public ParkingTicket? ParkingTicket { get; set; }


}
