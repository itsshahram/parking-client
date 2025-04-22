

using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Parking.ParkingSection;

public class ParkingSectionModel
{

    public Guid Id { get; set; }
    public short SectionNumber { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? IndexName { get; set; }
    public short? Floor { get; set; }
    public int Capacity { get; set; }
    public DateTime CreateDate { get; set; }
    public int ParkingId { get; set; }
}
