using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Vehicle.Vehicle;

public class CreateVehicleModel
{
    [MaxLength(50)]
    public string? LicensePlate { get; set; }
    public Guid? SystemId { get; set; }
    public short? SegmentId { get; set; }
    public string? SegmentName { get; set; }
    public string? ManufacturerName { get; set; }
    [MaxLength(150)]
    public string? Model { get; set; }
    [MaxLength(150)]
    public string? Color { get; set; }
}
