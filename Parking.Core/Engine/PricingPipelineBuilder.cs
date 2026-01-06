using Parking.Core.Models;
using Parking.Core.Rules;

namespace Parking.Core.Engine;

public class PricingPipelineBuilder
{
    private readonly Dictionary<PricingStage, List<IPricingRule>> _rules = new();

    public PricingPipelineBuilder()
    {
        foreach (PricingStage stage in Enum.GetValues(typeof(PricingStage)))
            _rules[stage] = new List<IPricingRule>();
    }

    public void AddRule(PricingStage stage, IPricingRule rule)
    {
        _rules[stage].Add(rule);
    }

    public PricingEngine Build() => new(_rules);
}