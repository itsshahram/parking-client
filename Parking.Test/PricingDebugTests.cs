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
        _output.WriteLine($"Daily Costs:");
        foreach (var dc in context.DailyCosts)
        {
            _output.WriteLine($"  {dc.Key:yyyy-MM-dd}: {dc.Value}");
        }
        _output.WriteLine($"Billable Minutes Per Day:");
        foreach (var bm in context.BillableMinutesPerDay)
        {
            _output.WriteLine($"  {bm.Key:yyyy-MM-dd}: {bm.Value} minutes");
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
        _output.WriteLine($"Daily Costs:");
        foreach (var dc in context.DailyCosts)
        {
            _output.WriteLine($"  {dc.Key:yyyy-MM-dd}: {dc.Value}");
        }
    }
}
