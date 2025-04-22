using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class ParkingTicketExtraImageModel
{
    public Guid TicketId { get; set; }
    public string? FaName { get; set; }
    public string? Image { get; set; }
    public string? GateName { get; set; }
    public bool ShowInPage { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public class ParkingTicketExtraImageSourceModel    : ParkingTicketExtraImageModel
{
    public ImageSource? ImageSource { get; set; }
}
