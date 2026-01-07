using Parking.Core.Models;

namespace Parking.Core.Rules;

public class FreeMinutesRule(
    int freeMinutes) : IPricingRule
{
    public string Name => $"زمان رایگان اولیه ({freeMinutes} دقیقه)";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED: Apply free minutes from entry time
        if (freeMinutes > 0)
        {
            // Calculate the end time of free period
            var freeEndTime = context.EntryTime.AddMinutes(freeMinutes);
            
            // Don't exceed exit time
            if (freeEndTime > context.ExitTime)
                freeEndTime = context.ExitTime;
            
            // Mark segments as charged (but with 0 amount) so other rules won't charge them
            TimeSegmentHelper.MarkSegmentsAsCharged(
                context.TimeSegments,
                context.EntryTime,
                freeEndTime,
                Name,
                0);
            
            context.AppliedRules.Add($"رایگان زمان اولیه ({freeMinutes} دقیقه) از {context.EntryTime:yyyy/MM/dd HH:mm}");
        }
        return Task.CompletedTask;
    }
}