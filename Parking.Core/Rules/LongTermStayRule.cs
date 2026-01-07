using Parking.Core.Models;

namespace Parking.Core.Rules;

/// <summary>
/// Long-term stay rule for vehicles parked for multiple consecutive days.
/// This rule applies a custom daily rate when the stay duration falls within a specified range.
/// For example: 10-20 days stay = $50/day, 21-30 days = $40/day, etc.
/// </summary>
public class LongTermStayRule(
    int minDays,
    int maxDays,
    decimal customDailyRate,
    decimal? hourlyRateForRemaining = null) : IPricingRule
{
    public string Name => "نرخ اقامت بلندمدت";

    public Task ApplyAsync(PricingContext context)
    {
        // Only execute on the first calendar day to avoid running multiple times
        if (context.CurrentDay != context.BillableMinutesPerDay.Keys.Min())
            return Task.CompletedTask;

        var totalDays = (int)context.TotalDuration.TotalDays;

        // Check if this stay qualifies for long-term rate based on total duration
        if (totalDays < minDays || (maxDays > 0 && totalDays > maxDays))
            return Task.CompletedTask;

        // Calculate how many full days worth of uncharged time we have
        var totalUnchargedMinutes = TimeSegmentHelper.GetUnchargedSegments(context.TimeSegments)
            .Sum(s => s.DurationMinutes);

        var fullDaysToCharge = totalUnchargedMinutes / 1440;
        
        // Double-check we have enough uncharged time to apply this rule
        if (fullDaysToCharge < minDays)
            return Task.CompletedTask;

        var totalCost = 0m;

        // Apply custom daily rate for each full day, capped at maxDays if specified
        var daysToCharge = maxDays > 0 ? Math.Min(fullDaysToCharge, maxDays) : fullDaysToCharge;
        
        for (int dayIndex = 0; dayIndex < daysToCharge; dayIndex++)
        {
            var periodStart = context.EntryTime.AddDays(dayIndex);
            var periodEnd = periodStart.AddDays(1);
            
            if (periodEnd > context.ExitTime)
                periodEnd = context.ExitTime;

            var unchargedMinutesInPeriod = context.GetUnchargedMinutesInDurationPeriod(dayIndex);
            
            // Only charge full days (at least 23 hours)
            if (unchargedMinutesInPeriod >= 1380)
            {
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    periodStart,
                    periodEnd,
                    Name,
                    customDailyRate);

                totalCost += customDailyRate;
            }
        }

        if (totalCost > 0)
        {
            context.AppliedRules.Add($"اقامت بلندمدت ({minDays}-{(maxDays > 0 ? maxDays : "نامحدود")} روز) - {daysToCharge} روز × {customDailyRate:N0} = {totalCost:N0}");
        }

        // Handle remaining partial day
        if (hourlyRateForRemaining.HasValue && hourlyRateForRemaining.Value > 0)
        {
            var lastFullDayEnd = context.EntryTime.AddDays(daysToCharge);
            if (lastFullDayEnd < context.ExitTime)
            {
                var unchargedSegments = TimeSegmentHelper.FilterSegmentsByTimeRange(
                    context.TimeSegments,
                    lastFullDayEnd,
                    context.ExitTime,
                    onlyUncharged: true);

                var remainingMinutes = unchargedSegments.Sum(s => s.DurationMinutes);
                if (remainingMinutes > 0)
                {
                    var remainingHours = Math.Ceiling(remainingMinutes / 60.0);
                    var remainingCost = (decimal)remainingHours * hourlyRateForRemaining.Value;

                    TimeSegmentHelper.MarkSegmentsAsCharged(
                        context.TimeSegments,
                        unchargedSegments,
                        Name,
                        remainingCost);

                    totalCost += remainingCost;
                    context.AppliedRules.Add($"زمان باقی‌مانده ({remainingMinutes} دقیقه) - نرخ ساعتی {remainingCost:N0}");
                }
            }
        }

        context.CurrentDayCost += totalCost;

        return Task.CompletedTask;
    }
}
