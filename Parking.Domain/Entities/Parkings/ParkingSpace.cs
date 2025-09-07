using System.ComponentModel.DataAnnotations;

namespace Parking.Domain.Entities.Parkings;

public class ParkingSpace
{
    [Key]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public int SpaceNumber { get; set; }
    public bool IsOccupied { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDate { get; set; }
    public int ParkingId { get; set; }
    public Guid ParkingSectionId { get; set; }
    //public Guid CreatorUserId { get; set; }

}
