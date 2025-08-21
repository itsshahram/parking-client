using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.Entities.Vehicles;

public class LicensePlateGroup
{
    [Key]
    public Guid Id { get; set; }
    public int ParkingLotId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public short DiscountPercent { get; set; }
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid CreatorUserId { get; set; }
    public virtual List<LicensePlate>? LicensePlates { get; set; }
}
