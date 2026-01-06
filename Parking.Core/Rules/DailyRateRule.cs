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
        var unchargedMinutes = context.GetUnchargedMinutesForCurrentDay();
        
        // Apply daily rate if there are enough uncharged minutes (close to a full day)
        if (unchargedMinutes >= 1380) // Allow some tolerance (23 hours)
        {
            // Determine which rate to use based on threshold
            var rate = dailyRate;
            if (thresholdDays > 0 && context.BillableMinutesPerDay.Count > thresholdDays)
            {
                rate = rateAfterThreshold > 0 ? rateAfterThreshold : dailyRate;
            }
            
            // Mark all uncharged segments for this day as charged
            var unchargedSegments = context.GetUnchargedSegmentsForCurrentDay();
            
            if (unchargedSegments.Count > 0)
            {
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    unchargedSegments,
                    Name,
                    rate);
                
                context.CurrentDayCost += rate;
                context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ روزانه {rate:N0}");
            }
        }
        
        return Task.CompletedTask;
    }
}