using Parking.Core.Models;

namespace Parking.Core.Rules;

// 1. هزینه ورود ثابت + کسر ۶۰ دقیقه اول
public class EntryFeeRule(
    decimal fee) : IPricingRule
{
    public string Name => $"هزینه ورود ({fee} تومان)";

    public Task ApplyAsync(PricingContext context)
    {
        if (fee > 0 && context.CurrentDay == context.EntryTime.Date)
        {
            context.CurrentDayCost += fee;
        }

        if (fee > 0 && context.CurrentDay == context.EntryTime.Date)
        {
            //context.CurrentDayBillableMinutes = Math.Max(0, context.CurrentDayBillableMinutes - 60);
            context.CurrentDayBillableMinutes = Math.Max(0, context.CurrentDayBillableMinutes);
        }

        return Task.CompletedTask;
    }
}