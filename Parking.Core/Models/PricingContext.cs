namespace Parking.Core.Models;

public class PricingContext
{
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public TimeSpan TotalDuration => ExitTime - EntryTime;
    public int TotalMinutes => (int)TotalDuration.TotalMinutes;

    public decimal BaseCost { get; set; } = 0;
    public decimal FinalCost { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;

    public Dictionary<DateTime, decimal> DailyCosts { get; set; } = new();
    public Dictionary<DateTime, int> BillableMinutesPerDay { get; set; } = new();
    public List<string> AppliedRules { get; set; } = new();

    // state جاری برای هر روز
    public DateTime CurrentDay { get; set; }
    public int CurrentDayBillableMinutes { get; set; }
    public decimal CurrentDayCost { get; set; } = 0;

    public void CalculateBaseDuration()
    {
        var current = EntryTime;
        var remainingMinutes = TotalMinutes;

        while (remainingMinutes > 0)
        {
            var day = current.Date;
            var minutesInDay = Math.Min(remainingMinutes, 1440 - (int)(current - day).TotalMinutes);
            BillableMinutesPerDay[day] = minutesInDay;
            DailyCosts[day] = 0;
            remainingMinutes -= minutesInDay;
            current = current.AddMinutes(minutesInDay);
        }
    }
}