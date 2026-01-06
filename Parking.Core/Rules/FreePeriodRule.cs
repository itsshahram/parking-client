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
        var isFree = false;

        if (from.HasValue && to.HasValue && context.CurrentDay >= from.Value.Date && context.CurrentDay <= to.Value.Date)
            isFree = true;

        if (specificDates?.Any(d => d.Date == context.CurrentDay.Date) == true)
            isFree = true;

        if (isFree)
        {
            // Mark all segments for the current day as free (charged with 0 amount)
            var dayStart = context.CurrentDay.Date;
            var dayEnd = dayStart.AddDays(1);
            
            TimeSegmentHelper.MarkSegmentsAsCharged(
                context.TimeSegments,
                dayStart,
                dayEnd,
                Name,
                0);
            
            context.CurrentDayCost = 0;
            context.CurrentDayBillableMinutes = 0;
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} رایگان شد");
        }

        return Task.CompletedTask;
    }
}