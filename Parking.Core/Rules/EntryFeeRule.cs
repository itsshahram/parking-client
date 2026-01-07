using Parking.Core.Models;

namespace Parking.Core.Rules;

// 1. هزینه ورود ثابت + کسر ۶۰ دقیقه اول
public class EntryFeeRule(
    decimal fee) : IPricingRule
{
    public string Name => $"هزینه ورود ({fee} تومان)";

    public Task ApplyAsync(PricingContext context)
    {
        // DURATION-BASED: Entry fee is applied once at the beginning
        if (fee > 0)
        {
            context.BaseCost += fee;
            context.AppliedRules.Add($"هزینه ورود ({fee:N0} تومان)");
        }

        return Task.CompletedTask;
    }
}