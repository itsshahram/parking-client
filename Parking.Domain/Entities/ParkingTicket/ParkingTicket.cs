using Parking.Domain.General;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Parking.Domain.Entities.ParkingTicket;

public class ParkingTicket
{
    [Key]
    public Guid Id { get; set; }
    [MaxLength(150)]
    public string? VehicleManufacturerName { get; set; }
    public int ParkingLotId { get; set; }
    public string? Description { get; set; }
    public long? CardUid { get; set; }
    [MaxLength(150)]
    public string? VehicleModel { get; set; }
    [MaxLength(150)]
    public string? VehicleColor { get; set; }
    public Guid? LicensePlateGroupId { get; set; }
    public string? LicensePlate { get; set; }
    public string? EnLicensePlate { get; set; }
    public long BarcodeId { get; set; }
    public int? VehicleSegmentId { get; set; }
    public Guid ParkingSpaceID { get; set; }
    public Guid ParkingSectionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? StartImage { get; set; }
    public bool IsExited { get; set; }
    public string? ExitImage { get; set; }
    public bool IsPaid { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalAmountWithDiscount { get; set; }
    public byte DiscountPercent { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaidCreditCard { get; set; }
    [MaxLength(150)]
    public string? PaidType { get; set; }
    public string? RefId { get; set; }
    public string? PaidDate { get; set; }
    public string? MerchantNumber { get; set; }
    public string? RRN { get; set; }
    public string? TraceNo { get; set; }
    public string? EntranceGate { get; set; }
    public string? ExitGate { get; set; }
    public bool? IsCardMissing { get; set; } = false;
    //ExtraInfo
    public string? DriverFullName { get; set; }
    public string? DriverPhoneNumber { get; set; }
    public string? DriverDescription { get; set; }


    public int? QueueNumber { get; set; }
    public int? TicketDescriptionItemId { get; set; }

    [ForeignKey(nameof(TicketDescriptionItemId))]
    public TicketDescriptionItem? TicketDescriptionItem { get; set; }


    public Guid? UserId { get; set; }
    public Guid? ExitRegistrarUserId { get; set; }
    public TicketStatus TicketStatus { get; set; }
    public required string DeviceId { get; set; }
    public required string IP { get; set; }
}
