using Parking.Core.Models;

namespace Parking.Core.Rules;

// 5. آستانه ساعتی (Duration-Based: applies to remaining uncharged time)
public class ThresholdHourlyRule(
    int thresholdHours, 
    decimal dailyRate) : IPricingRule
{
    public string Name => "آستانه ساعتی";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED IMPLEMENTATION
        // Check if remaining uncharged time meets threshold for daily rate
        
        var unchargedSegments = TimeSegmentHelper.GetUnchargedSegments(context.TimeSegments);
        var totalUnchargedMinutes = unchargedSegments.Sum(s => s.DurationMinutes);
        var unchargedHours = totalUnchargedMinutes / 60.0;
        
        if (unchargedHours >= thresholdHours && thresholdHours > 0)
        {
            if (unchargedSegments.Count > 0)
            {
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    unchargedSegments,
                    Name,
                    dailyRate);
                
                context.BaseCost += dailyRate;
                context.AppliedRules.Add($"زمان باقی‌مانده ({totalUnchargedMinutes} دقیقه) - یک روز کامل اضافه شد (آستانه {thresholdHours} ساعت)");
            }
        }
        
        return Task.CompletedTask;
    }
    
    /* OLD CALENDAR-BASED IMPLEMENTATION (COMMENTED OUT - DO NOT USE)
     * This only applied on the last calendar day which was inconsistent with duration-based approach
     * 
    public Task ApplyAsync(PricingContext context)
    {
        // Only apply on the last day
        if (context.CurrentDay != context.BillableMinutesPerDay.Keys.Max()) 
            return Task.CompletedTask;

        var unchargedMinutes = context.GetUnchargedMinutesForCurrentDay();
        var unchargedHours = unchargedMinutes / 60;
        
        if (unchargedHours >= thresholdHours && thresholdHours > 0)
        {
            // Mark all remaining uncharged segments for this day as charged
            var unchargedSegments = context.GetUnchargedSegmentsForCurrentDay();
            
            if (unchargedSegments.Count > 0)
            {
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    unchargedSegments,
                    Name,
                    dailyRate);
                
                context.CurrentDayCost += dailyRate;
                context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - یک روز کامل اضافه شد (آستانه {thresholdHours} ساعت)");
            }
        }
        
        return Task.CompletedTask;
    }
    */
}