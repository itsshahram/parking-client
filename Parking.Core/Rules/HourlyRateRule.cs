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
        if (context.CurrentDayBillableMinutes <= 0) 
            return Task.CompletedTask;

        var cost = 0m;
        var remaining = context.CurrentDayBillableMinutes;
        var currentTime = context.EntryTime.AddDays((context.CurrentDay - context.EntryTime.Date).Days);

        while (remaining > 0)
        {
            var segmentPrice = segmentPrices.FirstOrDefault(sp =>
                currentTime.TimeOfDay >= sp.TimeFrom.ToTimeSpan() &&
                currentTime.TimeOfDay < sp.TimeTo.ToTimeSpan());

            if (segmentPrice == null) 
                break;

            cost += segmentPrice.HourlyRate;
            currentTime = currentTime.AddHours(1);
            remaining -= 60;
        }

        context.CurrentDayCost += cost;
        if (cost > 0) 
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - نرخ ساعتی");
        
        return Task.CompletedTask;
    }
}