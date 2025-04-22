using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.Domain.Entities.ParkingTicket;

public class ParkingTicketImage
{
    [Key]
    public Guid TicketId { get; set; }
    public string? EntryImageAddress { get; set; }
    public string? ExitImageAddress { get; set; }
    public DateTime CreateDateTime { get; set; }
}
