using Parking.Core.Models;

namespace Parking.Core.Rules;

// 2. نرخ روزانه
public class DailyRateRule(
    decimal dailyRate, 
    decimal rateAfterThreshold = 0, 
    int thresholdDays = 0) : IPricingRule
{
    public string Name => "نرخ روزانه";

    public Task ApplyAsync(PricingContext context)
    {
        if (context.CurrentDayBillableMinutes >= 1440) // روز کامل
        {
            var rate = context.BillableMinutesPerDay.Count > thresholdDays ? rateAfterThreshold : dailyRate;
            context.CurrentDayCost += rate;
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ روزانه {rate}");
        }
        
        return Task.CompletedTask;
    }
}