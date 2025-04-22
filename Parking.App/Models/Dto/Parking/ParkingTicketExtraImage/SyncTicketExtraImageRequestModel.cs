using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingTicketExtraImage;

public class SyncTicketExtraImageRequestModel
{
    public Guid TicketId { get; set; }
    public string? FaName { get; set; }
    public required string Image { get; set; }
    public string? GateName { get; set; }
    public DateTime? CreateDateTime { get; set; }
}
