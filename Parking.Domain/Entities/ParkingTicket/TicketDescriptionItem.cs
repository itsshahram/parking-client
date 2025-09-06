using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.ParkingTicket;

public class TicketDescriptionItem
{
    [Key]
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsQueueEnabled { get; set; } = false;

}
