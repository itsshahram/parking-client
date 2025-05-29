using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Parking.App.Models.Tickets;


public class TicketsListViewModel
{
    public Guid Id { get; set; }
    public long? CardUid { get; set; }
    public string? VehicleManufacturerName { get; set; }
    public string? Description { get; set; }
    public long BarcodeId { get; set; }
    public int ParkingLotId { get; set; }
    public int? VehicleSegmentId { get; set; }
    public string? VehicleSegmentName { get; set; }
    public Guid ParkingSpaceID { get; set; }
    public string? ParkingSpaceName { get; set; }
    public string? LicensePlate { get; set; }
    public Guid? LicensePlateGroupId { get; set; }
    public string? LicensePlateGroupName { get; set; }
    public DateTime StartTime { get; set; }
    public string? StartRelativeTimeString { get; set; }
    public string? StartTimeString { get; set; }
    public string? StartTimeOnlyString { get; set; }
    public DateTime? EndTime { get; set; }
    public string? EndTimeString { get; set; }
    public string? EndTimeOnlyString { get; set; }
    public string? StartImage { get; set; }
    public BitmapImage? BitmapImage { get; set; }
    public bool? IsExited { get; set; }
    public string? ExitImage { get; set; }
    public bool? IsPaid { get; set; }
    public int DurationMinutes { get; set; }
    public decimal TotalAmount { get; set; }
    public byte DiscountPercent { get; set; }
    public decimal Discount { get; set; }
    public decimal PaidAmount { get; set; }
    public string? PaidCreditCard { get; set; }
    [MaxLength(150)]
    public string? PaidType { get; set; }
    public string? RefId { get; set; }
    public string? ParkingName { get; set; }
    public string? EnLicensePlate { get; set; }
    public string? PaidDate { get; set; }
    public string? MerchantNumber { get; set; }
    public string? RRN { get; set; }
    public string? TraceNo { get; set; }
    public string? ExtraInfo { get; set; }
    public bool IsSeized { get; set; } = false;
    public bool IsMissingCard { get; set; } = false;
    public string? EntranceGate { get; set; }
    public string? ExitGate { get; set; }

}
