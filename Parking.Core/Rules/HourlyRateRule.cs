using Parking.Core.Models;
using Parking.Domain.Entities.Parkings;

namespace Parking.Core.Rules;

// 3. نرخ ساعتی
public class HourlyRateRule(
    List<ParkingVehicleSegmentPrice> segmentPrices) : IPricingRule
{
    public string Name => "نرخ ساعتی";

    public Task ApplyAsync(PricingContext context)
    {
        var unchargedSegments = context.GetUnchargedSegmentsForCurrentDay();
        
        if (unchargedSegments.Count == 0) 
            return Task.CompletedTask;

        var cost = 0m;

        foreach (var segment in unchargedSegments)
        {
            var segmentCost = 0m;
            var current = segment.Start;
            var remaining = segment.DurationMinutes;

            while (remaining > 0)
            {
                // Find the matching segment price for current time
                var segmentPrice = segmentPrices.FirstOrDefault(sp =>
                    current.TimeOfDay >= sp.TimeFrom.ToTimeSpan() &&
                    current.TimeOfDay < sp.TimeTo.ToTimeSpan());

                if (segmentPrice == null)
                {
                    // No price defined for this time period, skip to next hour
                    current = current.AddHours(1);
                    remaining = (int)(segment.End - current).TotalMinutes;
                    if (remaining <= 0) break;
                    continue;
                }

                // Calculate how many minutes we can charge in this segment price period
                var segmentPriceEnd = current.Date.Add(segmentPrice.TimeTo.ToTimeSpan());
                var minutesInThisPrice = Math.Min(
                    remaining,
                    (int)(segmentPriceEnd - current).TotalMinutes
                );

                // Charge hourly rate for each full or partial hour
                var hours = Math.Ceiling(minutesInThisPrice / 60.0);
                segmentCost += (decimal)hours * segmentPrice.HourlyRate;

                current = current.AddMinutes(minutesInThisPrice);
                remaining -= minutesInThisPrice;
            }

            if (segmentCost > 0)
            {
                // Mark this segment as charged
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    new List<TimeSegment> { segment },
                    Name,
                    segmentCost);
                
                cost += segmentCost;
            }
        }

        context.CurrentDayCost += cost;
        
        if (cost > 0) 
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ ساعتی ({cost:N0} تومان)");
        
        return Task.CompletedTask;
    }
}