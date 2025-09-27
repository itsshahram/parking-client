
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class CreateParkingTicketModel
{
    [MaxLength(150)]
    public string? VehicleManufacturerName { get; set; }
    public string? Description { get; set; }
    [MaxLength(150)]
    public string? VehicleModel { get; set; }
    [MaxLength(150)]
    public string? VehicleColor { get; set; }
    public string? LicensePlate { get; set; }
    public string? EnLicensePlate { get; set; }
    public long? CardUid { get; set; }
    public int? VehicleSegmentId { get; set; }
    public Guid VehicleOwnerId { get; set; }
    public Guid ParkingSpaceID { get; set; }
    public Guid ParkingSectionId { get; set; }
    public string? StartImage { get; set; }
    public DateTime StartTime { get; set; }
    public string? EntranceGate { get; set; }
    //ExtraInfo
    public string? DriverFullName { get; set; }
    public string? DriverPhoneNumber { get; set; }
    public string? DriverDescription { get; set; }
    public int? TicketDescriptionItemId { get; set; }
    public Guid? CreatorUserId { get; set; }
}
