namespace Parking.WebApi.Helpers.PriceCalculation;

/// <summary>
/// مدل خروجی برای نمایش نتیجه محاسبه قیمت پارکینگ.
/// </summary>
public class ParkingPriceOutputResult
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

