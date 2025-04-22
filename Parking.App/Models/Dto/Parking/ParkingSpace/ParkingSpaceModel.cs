

namespace Parking.App.Models.Dto.Parking.ParkingSpace;

public class ParkingSpaceModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SpaceNumber { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public int ParkingId { get; set; }
    public Guid ParkingSectionId { get; set; }
}
