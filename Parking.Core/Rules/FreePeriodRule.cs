using Parking.Core.Models;

namespace Parking.Core.Rules;

// 6. قوانین ویژه – رایگان در بازه یا روز خاص
public class FreePeriodRule(
    DateTime? from, 
    DateTime? to, 
    List<DateTime>? specificDates = null) : IPricingRule
{
    public string Name => "قانون رایگان ویژه";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED: Check if the parking period overlaps with free periods
        var entryDate = context.EntryTime.Date;
        var exitDate = context.ExitTime.Date;
        
        // Process each calendar day that overlaps with the parking period
        for (var day = entryDate; day <= exitDate; day = day.AddDays(1))
        {
            var isFree = false;

            // Check if this day falls within the from/to range
            if (from.HasValue && to.HasValue && day >= from.Value.Date && day <= to.Value.Date)
                isFree = true;

            // Check if this day is in the specific dates list
            if (specificDates?.Any(d => d.Date == day) == true)
                isFree = true;

            if (isFree)
            {
                // Mark segments for this calendar day as free (charged with 0 amount)
                var dayStart = day;
                var dayEnd = day.AddDays(1);
                
                // Don't go before entry time or after exit time
                if (dayStart < context.EntryTime)
                    dayStart = context.EntryTime;
                if (dayEnd > context.ExitTime)
                    dayEnd = context.ExitTime;
                
                TimeSegmentHelper.MarkSegmentsAsCharged(
                    context.TimeSegments,
                    dayStart,
                    dayEnd,
                    Name,
                    0);
                
                context.AppliedRules.Add($"روز {day:yyyy/MM/dd} رایگان شد");
            }
        }

        return Task.CompletedTask;
    }
}