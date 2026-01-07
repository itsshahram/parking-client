using Parking.Core.Engine;
using Parking.Core.Models;
using Parking.Core.Rules;
using Parking.Domain.Entities.Parkings;
using Xunit;
using Xunit.Abstractions;

namespace Parking.Test;

public class PricingDebugTests
{
    private readonly ITestOutputHelper _output;

    public PricingDebugTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task Debug_HourlyAndDailyRules()
    {
        // Arrange
        var builder = new PricingPipelineBuilder();
        
        var segmentPrices = new List<ParkingVehicleSegmentPrice>
        {
            new ParkingVehicleSegmentPrice
            {
                HourlyRate = 5000,
                TimeFrom = new TimeOnly(0, 0),
                TimeTo = new TimeOnly(23, 59)
            }
        };
        // Daily rate should be added BEFORE hourly rate
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 0, 0, 0),
            ExitTime = new DateTime(2025, 10, 2, 0, 0, 0)
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Debug output
        _output.WriteLine($"Total Cost: {cost}");
        _output.WriteLine($"Base Cost: {context.BaseCost}");
        _output.WriteLine($"Segments: {context.TimeSegments.Count}");
        foreach (var seg in context.TimeSegments)
        {
            _output.WriteLine($"  {seg}");
        }
        _output.WriteLine($"Applied Rules:");
        foreach (var rule in context.AppliedRules)
        {
            _output.WriteLine($"  - {rule}");
        }
        
        // Debug: Output segment charges grouped by day
        _output.WriteLine($"Charges by Day:");
        var segmentsByDay = TimeSegmentHelper.GroupSegmentsByDay(context.TimeSegments);
        foreach (var dayGroup in segmentsByDay.OrderBy(g => g.Key))
        {
            var dayCost = dayGroup.Value.Where(s => s.IsCharged).Sum(s => s.ChargedAmount);
            var dayMinutes = dayGroup.Value.Sum(s => s.DurationMinutes);
            _output.WriteLine($"  {dayGroup.Key:yyyy-MM-dd}: {dayCost} ({dayMinutes} minutes)");
        }
    }
    
    [Fact]
    public async Task Debug_ComplexScenario()
    {
        // Arrange
        var builder = new PricingPipelineBuilder();
        
        // Entry fee
        builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(5000));
        
        // Free first 60 minutes
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(60));
        
        // Daily rate
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        
        // Hourly rate
        var segmentPrices = new List<ParkingVehicleSegmentPrice>
        {
            new ParkingVehicleSegmentPrice
            {
                HourlyRate = 10000,
                TimeFrom = new TimeOnly(0, 0),
                TimeTo = new TimeOnly(23, 59)
            }
        };
        builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 2, 10, 0, 0) // Exactly 24 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Debug output
        _output.WriteLine($"Total Cost: {cost}");
        _output.WriteLine($"Base Cost: {context.BaseCost}");
        _output.WriteLine($"Segments: {context.TimeSegments.Count}");
        foreach (var seg in context.TimeSegments)
        {
            _output.WriteLine($"  {seg}");
        }
        _output.WriteLine($"Applied Rules:");
        foreach (var rule in context.AppliedRules)
        {
            _output.WriteLine($"  - {rule}");
        }
        
        // Debug: Output segment charges grouped by day
        _output.WriteLine($"Charges by Day:");
        var segmentsByDay = TimeSegmentHelper.GroupSegmentsByDay(context.TimeSegments);
        foreach (var dayGroup in segmentsByDay.OrderBy(g => g.Key))
        {
            var dayCost = dayGroup.Value.Where(s => s.IsCharged).Sum(s => s.ChargedAmount);
            _output.WriteLine($"  {dayGroup.Key:yyyy-MM-dd}: {dayCost}");
        }
    }
}
