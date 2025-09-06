using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Tickets;

public class TicketDescriptionItemModel
{
    [Key]
    public int Id { get; set; }
    public string? Text { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsQueueEnabled { get; set; } = false;
}
