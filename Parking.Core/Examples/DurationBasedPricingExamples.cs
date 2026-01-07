using Parking.Core.Engine;
using Parking.Core.Models;
using Parking.Core.Rules;
using Parking.Domain.Entities.Parkings;

namespace Parking.Core.Examples;

/// <summary>
/// Practical examples demonstrating duration-based pricing rules.
/// These examples show real-world scenarios and how to configure the pricing engine.
/// </summary>
public static class DurationBasedPricingExamples
{
    /// <summary>
    /// Example 1: Basic duration-based pricing
    /// Problem: Entry at 10:12, Exit next day at 18:48
    /// Solution: Calculate based on full 24-hour periods from entry time
    /// </summary>
    public static async Task<decimal> Example1_BasicDurationBased()
    {
        var builder = new PricingPipelineBuilder();
        
        // Apply $50 per full day, $10/hour for remaining time
        builder.AddRule(PricingStage.BaseCalculate, 
            new DurationBasedDailyRateRule(
                dailyRate: 50000,
                hourlyRateForRemaining: 10000));
        
        var engine = builder.Build();
        
        var context = new PricingContext
        {
            EntryTime = new DateTime(2025, 10, 1, 10, 12, 0),
            ExitTime = new DateTime(2025, 10, 2, 18, 48, 0)
        };
        
        var cost = await engine.CalculateAsync(context);
        
        // Result: 140,000
        // Breakdown: 1 full day (50,000) + 9 hours (90,000) = 140,000
        return cost;
    }
    
    /// <summary>
    /// Example 2: Long-term stay with tiered pricing
    /// Scenario: Airport parking with discounts for longer stays
    /// </summary>
    public static PricingEngine Example2_AirportParkingWithTiers()
    {
        var builder = new PricingPipelineBuilder();
        
        // Entry fee (one-time charge)
        builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(10000));
        
        // First 30 minutes free
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(30));
        
        // Very long stays (30+ days): $20/day
        builder.AddRule(PricingStage.BaseCalculate, 
            new LongTermStayRule(
                minDays: 30,
                maxDays: 0, // 0 = unlimited
                customDailyRate: 20000,
                hourlyRateForRemaining: 5000));
        
        // Long stays (10-29 days): $30/day
        builder.AddRule(PricingStage.BaseCalculate, 
            new LongTermStayRule(
                minDays: 10,
                maxDays: 29,
                customDailyRate: 30000,
                hourlyRateForRemaining: 8000));
        
        // Short stays (< 10 days): $50/day
        builder.AddRule(PricingStage.BaseCalculate, 
            new DurationBasedDailyRateRule(
                dailyRate: 50000,
                hourlyRateForRemaining: 10000));
        
        // 9% tax
        builder.AddRule(PricingStage.Finalize, new TaxRule(0.09m));
        
        return builder.Build();
    }
    
    /// <summary>
    /// Example 3: Shopping mall parking
    /// - First 2 hours free
    /// - Then hourly up to 8 hours
    /// - After 8 hours, switch to daily rate
    /// - Multi-day stays get long-term discount
    /// </summary>
    public static PricingEngine Example3_ShoppingMallParking()
    {
        var builder = new PricingPipelineBuilder();
        
        // First 2 hours free
        builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(120));
        
        // Long-term discount for multi-day shoppers (3+ days)
        builder.AddRule(PricingStage.BaseCalculate, 
            new LongTermStayRule(
                minDays: 3,
                maxDays: 0,
                customDailyRate: 80000, // Discounted from 100000
                hourlyRateForRemaining: 15000));
        
        // Daily rate after 8 hours
        builder.AddRule(PricingStage.BaseCalculate, 
            new DurationBasedDailyRateRule(
                dailyRate: 100000,
                hourlyRateForRemaining: 20000,
                minHoursForDailyRate: 8)); // Switch to daily after 8 hours
        
        // Hourly rate for short stays
        var segmentPrices = new List<ParkingVehicleSegmentPrice>
        {
            new ParkingVehicleSegmentPrice
            {
                HourlyRate = 20000,
                TimeFrom = new TimeOnly(0, 0),
                TimeTo = new TimeOnly(23, 59)
            }
        };
        builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));
        
        return builder.Build();
    }
    
    /// <summary>
    /// Example 4: Hotel parking
    /// - Check-in at any time
    /// - Flat rate per night (24-hour period from check-in)
    /// - Discounted weekly rate
    /// - Discounted monthly rate
    /// </summary>
    public static PricingEngine Example4_HotelParking()
    {
        var builder = new PricingPipelineBuilder();
        
        // Monthly stays (30+ days): $15/day
        builder.AddRule(PricingStage.BaseCalculate, 
            new LongTermStayRule(
                minDays: 30,
                maxDays: 0,
                customDailyRate: 15000));
        
        // Weekly stays (7-29 days): $25/day
        builder.AddRule(PricingStage.BaseCalculate, 
            new LongTermStayRule(
                minDays: 7,
                maxDays: 29,
                customDailyRate: 25000));
        
        // Daily rate: $40/night (24-hour period)
        builder.AddRule(PricingStage.BaseCalculate, 
            new DurationBasedDailyRateRule(
                dailyRate: 40000,
                hourlyRateForRemaining: 10000,
                minHoursForDailyRate: 1)); // Even 1 hour counts as a full night
        
        return builder.Build();
    }
    
    /// <summary>
    /// Example 5: Comparison between calendar-based and duration-based
    /// This example demonstrates the problem with calendar-based calculations
    /// </summary>
    public static async Task<(decimal calendarCost, decimal durationCost)> Example5_ComparisonDemo()
    {
        // Setup: Entry at 10:12, Exit next day at 18:48 (~32.5 hours)
        var entryTime = new DateTime(2025, 10, 1, 10, 12, 0);
        var exitTime = new DateTime(2025, 10, 2, 18, 48, 0);
        
        // Calendar-based (OLD approach)
        var calendarBuilder = new PricingPipelineBuilder();
        calendarBuilder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
        var calendarEngine = calendarBuilder.Build();
        
        var calendarContext = new PricingContext
        {
            EntryTime = entryTime,
            ExitTime = exitTime
        };
        
        var calendarCost = await calendarEngine.CalculateAsync(calendarContext);
        
        // Duration-based (NEW approach)
        var durationBuilder = new PricingPipelineBuilder();
        durationBuilder.AddRule(PricingStage.BaseCalculate, 
            new DurationBasedDailyRateRule(50000, hourlyRateForRemaining: 10000));
        var durationEngine = durationBuilder.Build();
        
        var durationContext = new PricingContext
        {
            EntryTime = entryTime,
            ExitTime = exitTime
        };
        
        var durationCost = await durationEngine.CalculateAsync(durationContext);
        
        // Results:
        // Calendar-based: 0 (neither calendar day reaches 23-hour threshold)
        // Duration-based: 140,000 (1 full day @ 50,000 + 9 hours @ 10,000/hour)
        
        return (calendarCost, durationCost);
    }
}
