

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Parking.App.Models.Dto.Vehicle.VehicleSegment;

namespace Parking.App.Models.Dto.Parking.ParkingTicket;

public class ParkingTicketModel
{
    [Key]
    public Guid Id { get; set; }
    public Guid? LicensePlateGroupId { get; set; }
    public string? LicensePlateGroupName { get; set; }
    public string? VehicleManufacturerName { get; set; }
    public string? Description { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? LicensePlate { get; set; }
    public string? EnLicensePlate { get; set; }
    public long BarcodeId { get; set; }
    public int? VehicleSegmentId { get; set; }
    public string? VehicleSegmentName { get; set; }
    public Guid ParkingSpaceID { get; set; }
    public int ParkingId { get; set; }
    public string? ParkingName { get; set; }
    public Guid ParkingSectionId { get; set; }
    public DateTime StartTime { get; set; }
    public string? StartTimeString { get; set; }
    public DateTime? EndTime { get; set; }
    public string? EndTimeString { get; set; }
    public string? StartImage { get; set; }
    public bool IsExited { get; set; }
    public string? ExitImage { get; set; }
    public bool IsPaid { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public byte DiscountPercent { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaidCreditCard { get; set; }
    public string? PaidType { get; set; }
    public bool Synced { get; set; }
    public DateTime? SyncedTime { get; set; }
    public string? RefId { get; set; }
    public string? PaidDate { get; set; }
    public string? MerchantNumber { get; set; }
    public string? RRN { get; set; }
    public string? TraceNo { get; set; }
    public string? EntranceGate { get; set; }
    public string? ExitGate { get; set; }
    //ExtraInfo
    public string? DriverFullName { get; set; }
    public string? DriverPhoneNumber { get; set; }
    public string? DriverDescription { get; set; }
    public VehicleSegmentModel? VehicleSegment { get; set; }
}
