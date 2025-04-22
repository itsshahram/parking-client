using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Models.Dto.Vehicle.LicensePlate;

public class LicensePlateGroupModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public short DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid CreatorUserId { get; set; }
    public int ParkingLotId { get; set; }
    public List<LicensePlateModel>? LicensePlates { get; set; }
}
