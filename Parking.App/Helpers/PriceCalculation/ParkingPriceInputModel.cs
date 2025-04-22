using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Utilities.PriceCalculation;

/// <summary>
/// مدل ورودی برای محاسبه قیمت پارکینگ.
/// </summary>
public class ParkingPriceInputModel
{
    /// <summary>
    /// مبلغ ثابت ورودی پارکینگ.
    /// </summary>
    public decimal ParkingEntranceFixedFee { get; set; }
    /// <summary>
    /// مبلغ روزانه.
    /// </summary>
    public decimal DailyRate { get; set; }
    /// <summary>
    /// مبلغ ساعتی.
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// مبلغ ساعتی از
    /// </summary>
    public TimeOnly TimeFromHourlyRate { get; set; }
    /// <summary>
    /// مبلغ ساعتی تا
    /// </summary>
    public TimeOnly TimeToHourlyRate { get; set; }

    /// <summary>
    /// میزان دقیقه رایگان ورودی.
    /// </summary>
    public int FreeEntranceMinutes { get; set; }

    /// <summary>
    /// زمان ورود
    /// </summary>
    public DateTime StartDate { get; set; }
    /// <summary>
    /// زمان خروچ
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// درصد تخفیف.
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// تعداد ساعات کل برای محاسبه.
    /// </summary>
    public int TotalHours { get; set; }
}

