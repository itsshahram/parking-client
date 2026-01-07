using Parking.Core.Engine;
using Parking.Core.Models;
using Parking.Core.Rules;
using Xunit;
using Xunit.Abstractions;

namespace Parking.Test;

public class DurationBasedPricingTests
{
    private readonly ITestOutputHelper _output;

    public DurationBasedPricingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DurationBasedDailyRate_CalculatesFromEntryTime_NotCalendarDays()
    {
        // Arrange: Entry at 10:12, Exit next day at 18:48
        // This is ~32.5 hours total = 1 full 24-hour period + ~8.5 hours remaining
        var builder = new PricingPipelineBuilder();
        
        builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(
            dailyRate: 50000,
            hourlyRateForRemaining: 10000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 12, 0),
            ExitTime = new DateTime(2025, 10, 2, 18, 48, 0)
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Debug output
        _output.WriteLine($"Total Cost: {cost}");
        _output.WriteLine($"Total Duration: {context.TotalDuration}");
        _output.WriteLine($"Applied Rules:");
        foreach (var rule in context.AppliedRules)
        {
            _output.WriteLine($"  - {rule}");
        }
        
        // Assert
        // Should charge: 1 full day (50000) + 9 hours remaining (90000) = 140000
        Assert.Equal(140000, cost);
        Assert.Contains(context.AppliedRules, r => r.Contains("دوره 1"));
        Assert.Contains(context.AppliedRules, r => r.Contains("زمان باقی‌مانده"));
    }

    [Fact]
    public async Task DurationBasedDailyRate_ExactlyOneDayCalculatesCorrectly()
    {
        // Arrange: Entry at 10:00, Exit next day at 10:00 (exactly 24 hours)
        var builder = new PricingPipelineBuilder();
        
        builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(
            dailyRate: 50000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 2, 10, 0, 0)
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        Assert.Equal(50000, cost); // Exactly one daily rate
        Assert.Single(context.AppliedRules.Where(r => r.Contains("دوره")));
    }

    [Fact]
    public async Task DurationBasedDailyRate_MultipleDaysFromMidday()
    {
        // Arrange: Entry at 14:00, Exit 2.5 days later at 20:00
        var builder = new PricingPipelineBuilder();
        
        builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(
            dailyRate: 50000,
            hourlyRateForRemaining: 10000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 14, 0, 0),
            ExitTime = new DateTime(2025, 10, 4, 2, 0, 0) // 2 days and 12 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Debug output
        _output.WriteLine($"Total Cost: {cost}");
        _output.WriteLine($"Applied Rules:");
        foreach (var rule in context.AppliedRules)
        {
            _output.WriteLine($"  - {rule}");
        }
        
        // Assert
        // 2 full days (100000) + 12 hours (120000) = 220000
        Assert.Equal(220000, cost);
    }

    [Fact]
    public async Task LongTermStayRule_AppliesCustomRateForExtendedStays()
    {
        // Arrange: 15-day stay with long-term rate
        var builder = new PricingPipelineBuilder();
        
        builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(
            minDays: 10,
            maxDays: 20,
            customDailyRate: 30000, // Cheaper rate for long stays
            hourlyRateForRemaining: 5000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 16, 15, 0, 0) // 15 days + 5 hours
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Debug output
        _output.WriteLine($"Total Cost: {cost}");
        _output.WriteLine($"Total Duration Days: {context.TotalDurationDays}");
        _output.WriteLine($"Applied Rules:");
        foreach (var rule in context.AppliedRules)
        {
            _output.WriteLine($"  - {rule}");
        }
        
        // Assert
        // 15 full days × 30000 = 450000, plus 5 hours × 5000 = 25000, total = 475000
        Assert.Equal(475000, cost);
        Assert.Contains(context.AppliedRules, r => r.Contains("اقامت بلندمدت"));
    }

    [Fact]
    public async Task LongTermStayRule_DoesNotApplyIfDurationTooShort()
    {
        // Arrange: 5-day stay (below 10-day minimum)
        var builder = new PricingPipelineBuilder();
        
        builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(
            minDays: 10,
            maxDays: 20,
            customDailyRate: 30000));
        
        // Add fallback rule to charge something
        builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
            ExitTime = new DateTime(2025, 10, 6, 10, 0, 0) // 5 days
        };
        
        // Act
        var cost = await engine.CalculateAsync(context);
        
        // Assert
        // Should use standard daily rate, not long-term rate
        Assert.Equal(250000, cost); // 5 days × 50000
        Assert.DoesNotContain(context.AppliedRules, r => r.Contains("اقامت بلندمدت"));
    }

    [Fact]
    public async Task DurationBasedVsCalendarBased_ShowsDifference()
    {
        // This test demonstrates the difference between calendar-based and duration-based calculation
        
        // Arrange: Entry at 10:12, Exit next day at 18:48
        var entryTime = new DateTime(2025, 10, 1, 10, 12, 0);
        var exitTime = new DateTime(2025, 10, 2, 18, 48, 0);
        
        // Calendar-based approach (old DailyRateRule)
        var calendarBuilder = new PricingPipelineBuilder();
        calendarBuilder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        var calendarEngine = calendarBuilder.Build();
        
        var calendarContext = new PricingContext
        {
            EntryTime = entryTime,
            ExitTime = exitTime
        };
        
        var calendarCost = await calendarEngine.CalculateAsync(calendarContext);
        
        // Duration-based approach (new DurationBasedDailyRateRule)
        var durationBuilder = new PricingPipelineBuilder();
        durationBuilder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));
        var durationEngine = durationBuilder.Build();
        
        var durationContext = new PricingContext
        {
            EntryTime = entryTime,
            ExitTime = exitTime
        };
        
        var durationCost = await durationEngine.CalculateAsync(durationContext);
        
        // Debug output
        _output.WriteLine($"Calendar-based cost: {calendarCost}");
        _output.WriteLine("Calendar-based splits:");
        foreach (var day in calendarContext.BillableMinutesPerDay)
        {
            _output.WriteLine($"  {day.Key:yyyy-MM-dd}: {day.Value} minutes");
        }
        
        _output.WriteLine($"\nDuration-based cost: {durationCost}");
        _output.WriteLine($"Duration-based calculation: {durationContext.TotalDurationDays} full days + {durationContext.RemainingMinutesAfterFullDays} minutes");
        
        // Assert
        // Calendar-based won't apply daily rate (neither day has 1380+ minutes)
        // Duration-based will apply daily rate (has 1 full 24-hour period)
        _output.WriteLine($"\nCalendar applies daily rate: {calendarContext.AppliedRules.Any(r => r.Contains("روزانه"))}");
        _output.WriteLine($"Duration applies daily rate: {durationContext.AppliedRules.Any(r => r.Contains("دوره"))}");
        
        Assert.NotEqual(calendarCost, durationCost);
    }
}
