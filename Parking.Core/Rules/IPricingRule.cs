using Parking.Core.Models;

namespace Parking.Core.Rules;

public interface IPricingRule
{
    Task ApplyAsync(PricingContext context);
    string Name { get; }
}