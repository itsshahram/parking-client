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
        if (context.TotalMinutes >= 1440 || context.CurrentDay != context.EntryTime.Date)
            return Task.CompletedTask;

        var cost = 0m;
        var remaining = context.CurrentDayBillableMinutes;

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

        context.CurrentDayCost += cost;
        
        if (cost > 0) 
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ متغیر");
        
        return Task.CompletedTask;
    }
}