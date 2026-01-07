using Parking.Core.Models;

namespace Parking.Core.Rules;

// 2. نرخ روزانه (Duration-Based: calculates from entry time, not calendar days)
public class DailyRateRule(
    decimal dailyRate, 
    decimal rateAfterThreshold = 0, 
    int thresholdDays = 0) : IPricingRule
{
    public string Name => "نرخ روزانه";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED IMPLEMENTATION
        // Calculate based on full 24-hour periods from entry time
        
        var totalUnchargedMinutes = TimeSegmentHelper.GetUnchargedSegments(context.TimeSegments)
            .Sum(s => s.DurationMinutes);

        if (totalUnchargedMinutes < 1380) // Need at least 23 hours for daily rate
            return Task.CompletedTask;

        var fullDaysPossible = totalUnchargedMinutes / 1440;
        var totalCost = 0m;

        // Process each full 24-hour period from entry time
        for (int periodIndex = 0; periodIndex < fullDaysPossible; periodIndex++)
        {
            var periodStart = context.EntryTime.AddDays(periodIndex);
            var periodEnd = periodStart.AddDays(1);

            var unchargedMinutesInPeriod = TimeSegmentHelper.GetUnchargedMinutesInRange(
                context.TimeSegments, periodStart, periodEnd);
            
            if (unchargedMinutesInPeriod >= 1380) // 23 hours threshold
            {
                // Determine rate based on threshold days
                var rate = dailyRate;
                if (thresholdDays > 0 && periodIndex >= thresholdDays)
                {
                    rate = rateAfterThreshold > 0 ? rateAfterThreshold : dailyRate;
                }
                
                // Mark all uncharged segments in this 24-hour period as charged
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    periodStart,
                    periodEnd,
                    Name,
                    rate);

                totalCost += rate;
                context.AppliedRules.Add($"دوره {periodIndex + 1} (از {periodStart:yyyy/MM/dd HH:mm} تا {periodEnd:yyyy/MM/dd HH:mm}) - نرخ روزانه {rate:N0}");
            }
        }

        context.BaseCost += totalCost;
        return Task.CompletedTask;
    }
    
    /* OLD CALENDAR-BASED IMPLEMENTATION (COMMENTED OUT - DO NOT USE)
     * This approach was flawed because it used midnight boundaries instead of entry time
     * 
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
    */
}