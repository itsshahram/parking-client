using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Parking.App.Models.Dto.Vehicle.VehicleSegment;


public class VehicleSegmentPriceModel
{
    public int Id { get; set; }
    public int ParkingLotId { get; set; }
    public int VehicleSegmentId { get; set; }
    public string? VehicleSegmentName { get; set; }

    /// <summary>
    /// مبلغ ساعتی.
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// ساعت از
    /// </summary>
    public TimeOnly TimeFrom { get; set; }
    /// <summary>
    /// ساعت تا
    /// </summary>
    public TimeOnly TimeTo { get; set; }

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
}
