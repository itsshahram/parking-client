


using Parking.Domain.Entities.Parkings;
using System.ComponentModel.DataAnnotations;

namespace Parking.App.Models.Dto.Vehicle.VehicleSegment;

public class VehicleSegmentModel
{

    public int Id { get; set; }
    public string? NameFa { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public Guid? CreatorUserId { get; set; }
    public int ParkingLotId { get; set; }
    /// <summary>
    /// مبلغ ثابت ورودی پارکینگ.
    /// </summary>
    public decimal ParkingEntranceFixedFee { get; set; }
    /// <summary>
    /// مبلغ روزانه.
    /// </summary>
    public decimal DailyRate { get; set; }
    /// <summary>
    /// میزان دقیقه رایگان ورودی.
    /// </summary>
    public int FreeEntranceMinutes { get; set; }
    /// <summary>
    /// درصد مالیات
    /// </summary>
    public byte TaxPercentage { get; set; }
    /// <summary>
    /// آستانه ساعت در روز
    /// </summary>
    public byte ThresholdHoursPerDay { get; set; }
    /// <summary>
    /// آستانه تعداد روز
    /// </summary>
    public byte ThresholdNumberOfDays { get; set; }
    /// <summary>
    /// مبلغ روزانه بعد از رد کردن آستانه روز
    /// </summary>
    public decimal DailyPriceAfterCrossingThreshold { get; set; }
    public List<ParkingVehicleSegmentPriceModel>? VehicleSegmentPrices { get; set; }
    public List<ParkingVehicleSegmentVariablePrice>? VehicleSegmentVariablePrices { get; set; }
}
