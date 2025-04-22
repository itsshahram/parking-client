

namespace Parking.App.Models.Dto.Vehicle.VehicleOwner;

public class VehicleOwnerModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? NationalCode { get; set; }
    public string? Image { get; set; }
    public string? AdditionalImage { get; set; }
    public string? AdditionalImageTitle { get; set; }
    public Guid VehicleId { get; set; }
}
