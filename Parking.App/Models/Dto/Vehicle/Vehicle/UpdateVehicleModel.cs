

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Vehicle.Vehicle;

public class UpdateVehicleModel
{
    public Guid Id { get; set; }
    public string? LicensePlate { get; set; }
    public Guid? SystemId { get; set; }
    public string? QRCodeImage { get; set; }
    public short? SegmentId { get; set; }
    public string? ManufacturerName { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }
}
