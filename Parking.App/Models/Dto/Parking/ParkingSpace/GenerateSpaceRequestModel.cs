using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingSpace;

public class GenerateSpaceRequestModel
{
    public int ParkingLotId { get; set; }
    public int MinNumber { get; set; }
    public int MaxNumber { get; set; }
    public string? IndexName { get; set; }
    public short? Floor { get; set; }
    public Guid ParkingSectionId { get; set; }
    public int Count { get; set; }
}
