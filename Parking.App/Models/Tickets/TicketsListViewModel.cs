using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Tickets;


public class TicketsListViewModel
{
    public Guid Id { get; set; }
    public long? CardUid { get; set; }
    public string? VehicleManufacturerName { get; set; }
    public string? Description { get; set; }
    public string? DriverDescription { get; set; }
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
    public int? QueueNumber { get; set; }
    public bool? IsCustomPaid { get; set; }

}
public class TicketSummaryReportItem
{
    [DisplayName("تاریخ ورود")]
    public string StartTime { get; set; }

    [DisplayName("تاریخ خروج")]
    public string? EndTime { get; set; }

    [DisplayName("پلاک خودرو")]
    public string? LicensePlate { get; set; }

    [DisplayName("نوع خودرو")]
    public string? VehicleSegmentName { get; set; }

    [DisplayName("پارکینگ")]
    public string? ParkingName { get; set; }

    [DisplayName("درگاه ورود")]
    public string? EntranceGate { get; set; }

    [DisplayName("درگاه خروج")]
    public string? ExitGate { get; set; }

    [DisplayName("مدت زمان (دقیقه)")]
    public int DurationMinutes { get; set; }

    [DisplayName("مبلغ کل")]
    public decimal TotalAmount { get; set; }

    [DisplayName("تخفیف")]
    public decimal Discount { get; set; }

    [DisplayName("مبلغ پرداخت شده")]
    public decimal PaidAmount { get; set; }

    [DisplayName("نوع پرداخت")]
    public string? PaidType { get; set; }

    [DisplayName("پرداخت نقدی/کارت")]
    public string? PaidCreditCard { get; set; }

    [DisplayName("وضعیت پرداخت")]
    public string? IsPaid { get; set; }

    [DisplayName("خروج شده")]
    public string? IsExited { get; set; }

    [DisplayName("پرداخت دستی")]
    public string? IsCustomPaid { get; set; }
}