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
        // Variable rate only applies on the first day if total stay is less than 24 hours
        if (context.TotalMinutes >= 1440 || context.CurrentDay != context.EntryTime.Date)
            return Task.CompletedTask;

        var unchargedSegments = context.GetUnchargedSegmentsForCurrentDay();
        
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
        }

        context.CurrentDayCost += cost;
        
        if (cost > 0) 
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ متغیر ({cost:N0} تومان)");
        
        return Task.CompletedTask;
    }
}