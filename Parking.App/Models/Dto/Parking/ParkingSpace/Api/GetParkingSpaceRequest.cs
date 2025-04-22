using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingSpace.Api;

public class GetParkingSpaceRequest
{
    public int ParkingId { get; set; }
    public int? SectionId { get; set; }
    public bool? IsOccupied { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
