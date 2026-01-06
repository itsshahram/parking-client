using Parking.Core.Models;

namespace Parking.Core.Rules;

// 8. تخفیف درصدی کلی
public class PercentageDiscountRule(
    decimal percentage) : IPricingRule
{
    public string Name => $"تخفیف {percentage}%";

    public Task ApplyAsync(PricingContext context)
    {
        if (percentage > 0)
        {
            // decimal discount = context.BaseCost * (percentage / 100m);
            // context.DiscountAmount += discount;
            //
            // //context.DiscountAmount += context.BaseCost * (percentage / 100m);
            // context.FinalCost = context.BaseCost - context.DiscountAmount;
            // context.AppliedRules.Add(Name);
            
            
            context.DiscountAmount += context.BaseCost * (percentage / 100m);
            context.FinalCost = context.BaseCost - context.DiscountAmount;
            context.AppliedRules.Add(Name + $" (مقدار تخفیف: {context.DiscountAmount} تومان)");
        }
        
        return Task.CompletedTask;
    }
}