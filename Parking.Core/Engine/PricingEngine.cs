using Parking.Core.Models;
using Parking.Core.Rules;

namespace Parking.Core.Engine;

public class PricingEngine(Dictionary<PricingStage, List<IPricingRule>> rules)
{
    public async Task<decimal> CalculateAsync(PricingContext context)
    {
        // context.CalculateBaseDuration();
        //
        // foreach (var dayEntry in context.BillableMinutesPerDay.OrderBy(k => k.Key))
        // {
        //     context.CurrentDay = dayEntry.Key;
        //     context.CurrentDayBillableMinutes = dayEntry.Value;
        //     context.CurrentDayCost = 0;
        //
        //     foreach (PricingStage stage in Enum.GetValues(typeof(PricingStage)))
        //     {
        //         foreach (var rule in rules[stage])
        //         {
        //             await rule.ApplyAsync(context);
        //         }
        //     }
        //
        //     context.DailyCosts[context.CurrentDay] = context.CurrentDayCost;
        //     context.BaseCost += context.CurrentDayCost;
        // }
        //
        // // return context.FinalCost;
        // context.FinalCost = context.BaseCost - context.DiscountAmount + context.TaxAmount;
        //
        // return context.FinalCost;
        
        
        context.CalculateBaseDuration();
        foreach (var dayEntry in context.BillableMinutesPerDay.OrderBy(k => k.Key))
        {
            context.CurrentDay = dayEntry.Key;
            // Set initial billable minutes, rules will update based on segments
            context.CurrentDayBillableMinutes = context.GetUnchargedMinutesForCurrentDay();
            context.CurrentDayCost = 0;

            // فقط مراحل روزانه رو اجرا کن
            foreach (PricingStage stage in new[] { PricingStage.PreProcess, PricingStage.BaseCalculate, PricingStage.Adjust })
            {
                if (rules.TryGetValue(stage, out var stageRules))
                {
                    foreach (var rule in stageRules)
                    {
                        await rule.ApplyAsync(context);
                        // Update billable minutes after each rule
                        context.CurrentDayBillableMinutes = context.GetUnchargedMinutesForCurrentDay();
                    }
                }
            }

            context.DailyCosts[context.CurrentDay] = context.CurrentDayCost;
            context.BaseCost += context.CurrentDayCost;
        }

        // حالا مراحل کلی (Finalize) رو یک بار اجرا کن
        if (rules.TryGetValue(PricingStage.Finalize, out var finalizeRules))
        {
            foreach (var rule in finalizeRules)
            {
                await rule.ApplyAsync(context);
            }
        }
        
        // If FinalCost wasn't set by finalize rules, use BaseCost
        if (context.FinalCost == 0 && context.BaseCost > 0)
        {
            context.FinalCost = context.BaseCost;
        }

        return context.FinalCost;
    }
    
}