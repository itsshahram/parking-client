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
        if (dates.Any(d => d.Date == context.CurrentDay.Date))
        {
            context.CurrentDayCost *= multiplier;
            context.AppliedRules.Add($"روز {context.CurrentDay:yyyy/MM/dd} - ضریب {multiplier}x");
        }
        return Task.CompletedTask;
    }
}

// زمان رایگان اولیه – بروز با state روز به روز