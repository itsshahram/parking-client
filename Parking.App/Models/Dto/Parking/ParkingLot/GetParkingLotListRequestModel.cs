using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Parking.ParkingLot;

public class GetParkingLotListRequestModel
{
    public string? Name { get; set; }
    public short? Province { get; set; }
    public short? City { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsOnline { get; set; }
    public Guid? OwnerUserId { get; set; }
    public int? ParkingLotId { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxCapacity { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
