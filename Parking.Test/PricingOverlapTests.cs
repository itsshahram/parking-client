using Parking.Core.Engine;
using Parking.Core.Models;
using Parking.Core.Rules;
using Parking.Domain.Entities.Parkings;
using Parking.Domain.Entities.Vehicles;
using Xunit;

namespace Parking.Test;

public class PricingOverlapTests
{
    [Fact]
    public async Task FreeMinutesRule_DoesNotGetChargedByHourlyRule()
    {
        // Arrange: 30 minutes free + hourly rate
        var builder = new PricingPipelineBuilder();
        
        // Add free minutes rule (30 min)
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(30));
        
        // Add hourly rate rule
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
            ExitTime = new DateTime(2025, 10, 1, 11, 30, 0) // 90 minutes total
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Should charge only for 60 minutes (1 hour) after the 30 free minutes
        Assert.Equal(10000, cost); // 1 hour × 10000
        
        // Verify segments
        var chargedSegments = context.TimeSegments.Where(s => s.IsCharged).ToList();
        Assert.Equal(2, chargedSegments.Count); // Free segment + charged segment
        
        var freeSegment = chargedSegments.First(s => s.ChargedBy?.Contains("رایگان") == true);
        Assert.Equal(30, freeSegment.DurationMinutes);
        Assert.Equal(0, freeSegment.ChargedAmount);
    }

    [Fact]
    public async Task FreePeriodRule_MakesEntireDayFree_NoOtherRulesCharge()
    {
        // Arrange: Make Oct 1 free + add hourly and daily rules
        var builder = new PricingPipelineBuilder();
        
        var freeDate = new DateTime(2025, 10, 1);
        builder.AddRule(PricingStage.PreProcess, new FreePeriodRule(freeDate, freeDate));
        
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
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 1, 18, 0, 0) // 8 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        Assert.Equal(0, cost); // Should be free
        Assert.True(context.AppliedRules.Any(r => r.Contains("رایگان شد")));
        
        // All segments should be marked as charged with 0 amount
        var allCharged = context.TimeSegments.All(s => s.IsCharged);
        Assert.True(allCharged);
        Assert.All(context.TimeSegments, s => Assert.Equal(0, s.ChargedAmount));
    }

    [Fact]
    public async Task HourlyAndDailyRules_DoNotDoubleCharge()
    {
        // Arrange: Both hourly and daily rules configured
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
        
        // Daily rate should be added BEFORE hourly rate (as in real service)
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 0, 0, 0),
            ExitTime = new DateTime(2025, 10, 2, 0, 0, 0) // Exactly 24 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Daily rule should charge 50000, hourly rule should not charge because all segments are charged
        Assert.Equal(50000, cost);
        
        // Verify all segments are charged
        var allCharged = context.TimeSegments.All(s => s.IsCharged);
        Assert.True(allCharged);
    }

    [Fact]
    public async Task VariableAndHourlyRules_DoNotDoubleCharge()
    {
        // Arrange: Both variable and hourly rules configured
        var builder = new PricingPipelineBuilder();
        
        var variablePrices = new List<ParkingVehicleSegmentVariablePrice>
        {
            new ParkingVehicleSegmentVariablePrice { Number = 1, Minutes = 60, Price = 5000 },
            new ParkingVehicleSegmentVariablePrice { Number = 2, Minutes = 60, Price = 7000 },
            new ParkingVehicleSegmentVariablePrice { Number = 3, Minutes = 60, Price = 9000 }
        };
        builder.AddRule(PricingStage.BaseCalculate, new VariableRateRule(variablePrices));
        
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
            ExitTime = new DateTime(2025, 10, 1, 13, 0, 0) // 3 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Variable rate should charge: 5000 + 7000 + 9000 = 21000
        // Hourly rate should not charge because segments are already charged
        Assert.Equal(21000, cost);
        
        // Verify all segments are charged by variable rate
        var allCharged = context.TimeSegments.All(s => s.IsCharged);
        Assert.True(allCharged);
    }

    [Fact]
    public async Task MultiDayParking_WithFreeFirstDay_ChargesOnlySecondDay()
    {
        // Arrange
        var builder = new PricingPipelineBuilder();
        
        // Make Oct 1 free
        var freeDate = new DateTime(2025, 10, 1);
        builder.AddRule(PricingStage.PreProcess, new FreePeriodRule(freeDate, freeDate));
        
        // Add daily rate
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 0, 0, 0),
            ExitTime = new DateTime(2025, 10, 3, 0, 0, 0) // Exactly 2 full days
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Oct 1: Free (0), Oct 2: 50000 = 50000 total (Oct 3 has 0 minutes)
        Assert.Equal(50000, cost);
        
        // Verify daily costs
        Assert.Equal(0, context.DailyCosts[new DateTime(2025, 10, 1)]);
        Assert.Equal(50000, context.DailyCosts[new DateTime(2025, 10, 2)]);
    }

    [Fact]
    public async Task ThresholdHourlyRule_OnlyChargesRemainingUnchargedHours()
    {
        // Arrange
        var builder = new PricingPipelineBuilder();
        
        // Free first 2 hours
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(120));
        
        // Threshold: if >= 6 hours, charge daily rate
        builder.AddRule(PricingStage.Adjust, new ThresholdHourlyRule(6, 50000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 1, 18, 0, 0) // 8 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // 2 hours free, 6 hours remaining (>= threshold), so charge daily rate
        Assert.Equal(50000, cost);
        
        // All segments should be charged
        var allCharged = context.TimeSegments.All(s => s.IsCharged);
        Assert.True(allCharged);
    }

    [Fact]
    public async Task EntryFee_Plus_FreeMinutes_Plus_HourlyRate_WorkCorrectly()
    {
        // Arrange
        var builder = new PricingPipelineBuilder();
        
        // Entry fee: 5000
        builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(5000));
        
        // Free first 30 minutes
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(30));
        
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
            ExitTime = new DateTime(2025, 10, 1, 12, 30, 0) // 2.5 hours = 150 minutes
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Entry fee: 5000
        // Free: 30 minutes
        // Charged: 120 minutes = 2 hours × 10000 = 20000
        // Total: 5000 + 20000 = 25000
        Assert.Equal(25000, cost);
    }

    [Fact]
    public async Task ComplexScenario_MultiDay_WithVariousRules_NoDoubleCharging()
    {
        // Arrange: Simpler scenario to verify no double-charging
        var builder = new PricingPipelineBuilder();
        
        // Entry fee
        builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(5000));
        
        // Free first 60 minutes
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(60));
        
        // Daily rate - but won't apply because we don't have full days after free time
        builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        
        // Hourly rate (as fallback for incomplete days)
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
        
        // Assert
        // Entry: 5000
        // 24 hours total - 1 hour free = 23 hours
        // Oct 1: 13 hours (11:00-00:00) = 130000
        // Oct 2: 10 hours (00:00-10:00) = 100000
        // Total: 5000 + 230000 = 235000
        Assert.Equal(235000, cost);
        
        // Verify no actual double-charging by checking that base cost matches segment total
        var totalChargedFromSegments = TimeSegmentHelper.GetTotalChargedAmount(context.TimeSegments);
        
        // Entry fee is not tracked in segments (it's not time-based), so add it
        var expectedSegmentTotal = context.BaseCost - 5000; // BaseCost includes entry fee
        Assert.True(Math.Abs(expectedSegmentTotal - totalChargedFromSegments) < 1, 
            $"Segment total ({totalChargedFromSegments}) should match BaseCost minus entry fee ({expectedSegmentTotal})");
    }
}
