using Parking.Core.Models;

namespace Parking.Core.Rules;

public class FreeMinutesRule(
    int freeMinutes) : IPricingRule
{
    public string Name => $"زمان رایگان اولیه ({freeMinutes} دقیقه)";

    public Task ApplyAsync(PricingContext context)
    {
        // فقط در روز اول ورود اعمال می‌شه
        if (context.CurrentDay == context.EntryTime.Date)
        {
            if (context.CurrentDayBillableMinutes <= freeMinutes)
            {
                context.CurrentDayCost = 0;
                context.CurrentDayBillableMinutes = 0;
                context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - رایگان زمان اولیه");
            }
        }
        return Task.CompletedTask;
    }
}