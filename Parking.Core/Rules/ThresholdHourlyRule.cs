using Parking.Core.Models;

namespace Parking.Core.Rules;

// 5. آستانه ساعتی (در آخرین روز)
public class ThresholdHourlyRule(
    int thresholdHours, 
    decimal dailyRate) : IPricingRule
{
    public string Name => "آستانه ساعتی";

    public Task ApplyAsync(PricingContext context)
    {
        if (context.CurrentDay != context.BillableMinutesPerDay.Keys.Max()) 
            return Task.CompletedTask;

        if (context.CurrentDayBillableMinutes / 60 >= thresholdHours && thresholdHours > 0)
        {
            context.CurrentDayCost += dailyRate;
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - یک روز کامل اضافه شد (آستانه {thresholdHours} ساعت)");
        }
        
        return Task.CompletedTask;
    }
}