using Parking.Core.Models;
using Parking.Domain.Entities.Parkings;

namespace Parking.Core.Rules;

// 4. نرخ متغیر (پلکانی)
public class VariableRateRule(
    List<ParkingVehicleSegmentVariablePrice> variablePrices) : IPricingRule
{
    public string Name => "نرخ متغیر";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED: Variable rate only applies on short stays (less than 24 hours)
        // and only if there are uncharged segments
        if (context.TotalMinutes >= 1440)
            return Task.CompletedTask;

        var unchargedSegments = TimeSegmentHelper.GetUnchargedSegments(context.TimeSegments);
        
        if (unchargedSegments.Count == 0)
            return Task.CompletedTask;

        var cost = 0m;
        var totalUnchargedMinutes = unchargedSegments.Sum(s => s.DurationMinutes);
        var remaining = totalUnchargedMinutes;

        // Apply variable pricing tiers
        foreach (var variablePrice in variablePrices.OrderBy(v => v.Number))
        {
            if (remaining <= 0)
                break;

            if (remaining > variablePrice.Minutes)
            {
                cost += variablePrice.Price ?? 0;
                remaining -= variablePrice.Minutes;
            }
            else
            {
                cost += (variablePrice.Price ?? 0) * (remaining / (decimal)variablePrice.Minutes);
                remaining = 0;
            }
        }

        if (cost > 0)
        {
            // Mark all uncharged segments as charged
            TimeSegmentHelper.MarkSegmentsAsCharged(
                context.TimeSegments,
                unchargedSegments,
                Name,
                cost);
                
            context.BaseCost += cost;
            context.AppliedRules.Add($"نرخ متغیر ({cost:N0} تومان) - {totalUnchargedMinutes} دقیقه");
        }
        
        return Task.CompletedTask;
    }
}