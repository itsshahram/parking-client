using System.Text.Json;
using Parking.Core.Engine;
using Parking.Core.Models;
using Parking.Core.Rules;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.Vehicles;

namespace Parking.Core.Service;

public class ParkingPricingService : IParkingPriceService
{
    public Task<PricingEngine> CreateEngine(
        VehicleSegment segment,
        List<SpecialRule> specialRules,
        List<ParkingVehicleSegmentPrice> segmentPrices,
        List<ParkingVehicleSegmentVariablePrice> variablePrices,
        decimal discountPercentage)
    {
        // مرتب‌سازی قوانین ویژه بر اساس Priority
        var orderedSpecialRules = specialRules?
            .OrderBy(x => x.Priority)
            .ToList() ?? new List<SpecialRule>();

        var builder = new PricingPipelineBuilder();

        // PreProcess: قوانین رایگان ویژه
        foreach (var rule in orderedSpecialRules.Where(r => r.MakeFree))
        {
            List<DateTime>? dates = null;
            if (!string.IsNullOrEmpty(rule.SpecificDatesJson))
                dates = JsonSerializer.Deserialize<List<DateTime>>(rule.SpecificDatesJson);

            builder.AddRule(PricingStage.PreProcess, new FreePeriodRule(rule.FromDate, rule.ToDate, dates));
        }

        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(segment.FreeEntranceMinutes));

        // BaseCalculate: قوانین پایه
        builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(segment.ParkingEntranceFixedFee));
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(
            segment.DailyRate,
            segment.DailyPriceAfterCrossingThreshold,
            segment.ThresholdNumberOfDays));

        if (variablePrices.Count != 0)
            builder.AddRule(PricingStage.BaseCalculate, new VariableRateRule(variablePrices));
        else if (segmentPrices.Count != 0)
            builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));

        // Adjust: آستانه و ضریب‌ها
        if (segment.ThresholdHoursPerDay > 0)
            builder.AddRule(PricingStage.Adjust, new ThresholdHourlyRule(segment.ThresholdHoursPerDay, segment.DailyRate));

        foreach (var rule in orderedSpecialRules.Where(r => r.RateMultiplier.HasValue))
        {
            List<DateTime>? dates = null;
            if (!string.IsNullOrEmpty(rule.SpecificDatesJson))
                dates = JsonSerializer.Deserialize<List<DateTime>>(rule.SpecificDatesJson);

            builder.AddRule(PricingStage.Adjust, new MultiplierRateRule(dates ?? new List<DateTime>(), rule.RateMultiplier.Value));
        }

        // Finalize
        if (discountPercentage > 0)
            builder.AddRule(PricingStage.Finalize, new PercentageDiscountRule(discountPercentage));

        builder.AddRule(PricingStage.Finalize, new TaxRule(segment.TaxPercentage));

        return Task.FromResult(builder.Build());
    }
}