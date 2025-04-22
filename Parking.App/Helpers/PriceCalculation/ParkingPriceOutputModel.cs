using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parking.App.Utilities.PriceCalculation;

/// <summary>
/// مدل خروجی برای نمایش نتیجه محاسبه قیمت پارکینگ.
/// </summary>
public class ParkingPriceOutputModel
{
    /// <summary>
    /// مبلغ کل بدون تخفیف.
    /// </summary>
    public decimal TotalWithoutDiscount { get; set; }

    /// <summary>
    /// مبلغ تخفیف.
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// مبلغ قابل پرداخت.
    /// </summary>
    public decimal PayableAmount { get; set; }

    /// <summary>
    /// تعداد روزهای کل.
    /// </summary>
    public int TotalDays { get; set; }

    /// <summary>
    /// تعداد ساعات کل.
    /// </summary>
    public int TotalHours { get; set; }

    /// <summary>
    /// تعداد دقایق کل.
    /// </summary>
    public int TotalMinutes { get; set; }
}

