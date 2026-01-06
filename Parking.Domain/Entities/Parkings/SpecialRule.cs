namespace Parking.Domain.Entities.Parkings;

public class SpecialRule
{
    public int Id { get; set; }
    public string RuleType { get; set; } = string.Empty; // FreePeriod, PercentageDiscount, MultiplierRate, etc.
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SpecificDatesJson { get; set; } // JSON لیست تاریخ‌ها: ["2025-12-21"]
    public string? DaysOfWeekJson { get; set; } // ["Friday", "Saturday"]
    public TimeSpan? FromTimeOfDay { get; set; }
    public TimeSpan? ToTimeOfDay { get; set; }
    public int? MinDurationMinutes { get; set; }
    //public string? VehicleType { get; set; } // "Car", "Electric", "SUV"

    // عملیات
    public bool MakeFree { get; set; } = false;
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public decimal? RateMultiplier { get; set; }
    public decimal? MaxDailyCharge { get; set; }
    public decimal? MinCharge { get; set; }

    public int Priority { get; set; } = 100; // عدد کمتر = اولویت بالاتر
}