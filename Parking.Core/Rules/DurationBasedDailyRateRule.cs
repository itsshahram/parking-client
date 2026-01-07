using Parking.Core.Models;

namespace Parking.Core.Rules;

/// <summary>
/// Daily rate rule based on full 24-hour periods from entry time, not calendar days.
/// This rule calculates based on the vehicle's actual entry time as the reference point.
/// For example: Entry at 10:12, Exit next day at 18:48 = one full 24-hour period + remaining hours.
/// </summary>
public class DurationBasedDailyRateRule(
    decimal dailyRate,
    decimal? hourlyRateForRemaining = null,
    int minHoursForDailyRate = 23) : IPricingRule
{
    public string Name => "نرخ روزانه مبتنی بر مدت";

    public Task ApplyAsync(PricingContext context)
    {
        // This rule should run once for the entire duration, not per calendar day
        // Only execute on the first calendar day to avoid running multiple times
        if (context.CurrentDay != context.BillableMinutesPerDay.Keys.Min())
            return Task.CompletedTask;

        var totalUnchargedMinutes = TimeSegmentHelper.GetUnchargedSegments(context.TimeSegments)
            .Sum(s => s.DurationMinutes);

        if (totalUnchargedMinutes < minHoursForDailyRate * 60)
            return Task.CompletedTask;

        var fullDaysPossible = totalUnchargedMinutes / 1440; // Full 24-hour periods
        var totalCost = 0m;

        // Process each full 24-hour period from entry time
        for (int periodIndex = 0; periodIndex < fullDaysPossible; periodIndex++)
        {
            var periodStart = context.EntryTime.AddDays(periodIndex);
            var periodEnd = periodStart.AddDays(1);

            var unchargedMinutesInPeriod = context.GetUnchargedMinutesInDurationPeriod(periodIndex);
            
            // Check if this period has enough uncharged time for daily rate
            if (unchargedMinutesInPeriod >= minHoursForDailyRate * 60)
            {
                // Mark all uncharged segments in this 24-hour period as charged
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    periodStart,
                    periodEnd,
                    Name,
                    dailyRate);

                totalCost += dailyRate;
                context.AppliedRules.Add($"دوره {periodIndex + 1} (از {periodStart:yyyy/MM/dd HH:mm} تا {periodEnd:yyyy/MM/dd HH:mm}) - نرخ روزانه {dailyRate:N0}");
            }
        }

        // Handle remaining time after full days
        if (hourlyRateForRemaining.HasValue && hourlyRateForRemaining.Value > 0)
        {
            var lastFullPeriodEnd = context.EntryTime.AddDays(fullDaysPossible);
            if (lastFullPeriodEnd < context.ExitTime)
            {
                var unchargedSegments = TimeSegmentHelper.FilterSegmentsByTimeRange(
                    context.TimeSegments,
                    lastFullPeriodEnd,
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

        // Add to the current day cost (first day) since we process everything at once
        context.CurrentDayCost += totalCost;

        return Task.CompletedTask;
    }
}
