using Parking.Core.Models;

namespace Parking.Core.Rules;

public class FreeMinutesRule(
    int freeMinutes) : IPricingRule
{
    public string Name => $"زمان رایگان اولیه ({freeMinutes} دقیقه)";

    public Task ApplyAsync(PricingContext context)
    {
        // فقط در روز اول ورود اعمال می‌شه
        if (context.CurrentDay == context.EntryTime.Date && freeMinutes > 0)
        {
            // Calculate the end time of free period
            var freeEndTime = context.EntryTime.AddMinutes(freeMinutes);
            
            // Mark segments as charged (but with 0 amount) so other rules won't charge them
            TimeSegmentHelper.MarkSegmentsAsCharged(
                context.TimeSegments,
                context.EntryTime,
                freeEndTime,
                Name,
                0);
            
            // Update uncharged minutes for current day
            context.CurrentDayBillableMinutes = context.GetUnchargedMinutesForCurrentDay();
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - رایگان زمان اولیه ({freeMinutes} دقیقه)");
        }
        return Task.CompletedTask;
    }
}