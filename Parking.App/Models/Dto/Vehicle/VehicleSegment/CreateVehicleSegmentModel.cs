using Microsoft.AspNetCore.Http;

namespace Parking.App.Models.Dto.Vehicle.VehicleSegment;

public class CreateVehicleSegmentModel
{
    public string? NameFa { get; set; }
    public string? Description { get; set; }
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
    public IFormFile? Image { get; set; }
}
