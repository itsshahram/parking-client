using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.ParkingTicket;

public class TicketQueueResetPolicy
{
    [Key]
    public int Id { get; set; }

    public int TicketDescriptionItemId { get; set; }

    [ForeignKey(nameof(TicketDescriptionItemId))]
    public TicketDescriptionItem? TicketDescriptionItem { get; set; }

    public int ResetIntervalDays { get; set; } 
}
