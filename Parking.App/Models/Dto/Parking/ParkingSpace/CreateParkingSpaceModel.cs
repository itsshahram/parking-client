

namespace Parking.App.Models.Dto.Parking.ParkingSpace;

public class CreateParkingSpaceModel
{
    public string? Name { get; set; }
    public int SpaceNumber { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public int ParkingId { get; set; }
    public Guid ParkingSectionId { get; set; }
}
