using Parking.Core.Models;

namespace Parking.Core.Rules;

// 9. مالیات
public class TaxRule(
    decimal percentage) : IPricingRule
{
    public string Name => $"مالیات {percentage}%";

    public Task ApplyAsync(PricingContext context)
    {
        // var baseForTax = context.FinalCost > 0 ? context.FinalCost : context.BaseCost;
        // context.TaxAmount = baseForTax * (percentage / 100m);
        // context.FinalCost = baseForTax + context.TaxAmount;
        
        var baseForTax = context.BaseCost - context.DiscountAmount;  // بعد از تخفیف
        context.TaxAmount = baseForTax * (percentage / 100m);
        context.FinalCost = baseForTax + context.TaxAmount;
        
        return Task.CompletedTask;
    }
}