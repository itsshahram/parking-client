using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingSpace;

public class ChangeSpaceStatusModel
{
    public Guid SpaceId { get; set; }
    public bool IsOccupied { get; set; }
}
