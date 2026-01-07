using Parking.Core.Models;

namespace Parking.Core.Rules;

// 7. نرخ ضریب‌دار (مثل دو برابر)
public class MultiplierRateRule(
    List<DateTime> dates, 
    decimal multiplier) : IPricingRule
{
    public string Name => $"ضریب نرخ {multiplier}x";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED: Apply multiplier to segments on specific dates
        var entryDate = context.EntryTime.Date;
        var exitDate = context.ExitTime.Date;
        
        // Check each calendar day that overlaps with the parking period
        for (var day = entryDate; day <= exitDate; day = day.AddDays(1))
        {
            if (dates.Any(d => d.Date == day))
            {
                // Find all segments that fall on this calendar day
                var dayStart = day;
                var dayEnd = day.AddDays(1);
                
                // Don't go before entry time or after exit time
                if (dayStart < context.EntryTime)
                    dayStart = context.EntryTime;
                if (dayEnd > context.ExitTime)
                    dayEnd = context.ExitTime;
                
                // Get all segments in this day and apply multiplier to already charged ones
                var segmentsInDay = context.TimeSegments
                    .Where(s => s.Start < dayEnd && s.End > dayStart && s.IsCharged && s.ChargedAmount > 0)
                    .ToList();
                
                foreach (var segment in segmentsInDay)
                {
                    var additionalCost = segment.ChargedAmount * (multiplier - 1);
                    segment.ChargedAmount *= multiplier;
                    context.BaseCost += additionalCost;
                }
                
                if (segmentsInDay.Any())
                {
                    context.AppliedRules.Add($"روز {day:yyyy/MM/dd} - ضریب {multiplier}x اعمال شد");
                }
            }
        }
        
        return Task.CompletedTask;
    }
}