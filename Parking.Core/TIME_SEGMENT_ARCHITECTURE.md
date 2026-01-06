# Time Segment-Based Pricing System

## Overview

This document describes the time segment-based pricing system implemented to handle overlapping pricing rules in the parking price calculation engine.

## Problem Statement

The previous implementation had a critical flaw: **multiple pricing rules could charge for the same time period**, leading to double-charging and incorrect final prices.

### Issues with the Old System

1. **No Time Tracking**: Rules processed entire days without tracking which time periods had already been charged
2. **No Mutual Exclusion**: Multiple rules (e.g., HourlyRateRule and DailyRateRule) could both charge for the same time
3. **Overlapping Free Periods**: Free time wasn't properly excluded from subsequent rule calculations
4. **Rule Order Dependency**: Results varied based on the order rules were added to the pipeline

### Example Problem Scenario

```
Entry: Oct 1, 10:00
Exit: Oct 1, 13:00 (3 hours total)
Rules:
- First 30 minutes free
- Hourly rate: $10/hour

Old System Result:
- FreeMinutesRule: marks first 30 min as free
- HourlyRateRule: charges for ALL 3 hours = $30

Expected Result:
- First 30 minutes: FREE
- Remaining 2.5 hours: $30
- Total: $30 ✗ (but system was still charging for the free time!)
```

## Solution: Time Segment Tracking

### Core Concept

The parking duration is divided into **time segments**. Each segment represents a continuous period that can be:
- **Uncharged**: Available for rules to process
- **Charged**: Already processed by a rule (with tracked amount and rule name)

### Key Components

#### 1. TimeSegment Model (`TimeSegment.cs`)

```csharp
public class TimeSegment
{
    public DateTime Start { get; set; }          // Inclusive
    public DateTime End { get; set; }            // Exclusive
    public int DurationMinutes { get; }          // Calculated
    public bool IsCharged { get; set; }          // Processing state
    public string? ChargedBy { get; set; }       // Which rule charged this
    public decimal ChargedAmount { get; set; }   // Amount charged
}
```

#### 2. TimeSegmentHelper (`TimeSegmentHelper.cs`)

Provides utility methods for:
- **Initialization**: `InitializeSegments(entryTime, exitTime)` - creates initial uncharged segments
- **Querying**: `GetUnchargedSegments()`, `GetUnchargedMinutesForDay()`
- **Marking**: `MarkSegmentsAsCharged()` - marks segments as processed
- **Splitting**: Automatically splits segments when rules charge partial overlaps

#### 3. Updated PricingContext

```csharp
public class PricingContext
{
    // ... existing properties ...
    
    public List<TimeSegment> TimeSegments { get; set; } = new();
    
    public int GetUnchargedMinutesForCurrentDay() { ... }
    public List<TimeSegment> GetUnchargedSegmentsForCurrentDay() { ... }
}
```

## How It Works

### 1. Initialization

When pricing calculation starts:
```csharp
context.CalculateBaseDuration();
// Creates initial segments split by day boundaries
// All segments start as "uncharged"
```

### 2. Rule Processing

Each rule follows this pattern:
1. Query for uncharged segments
2. Calculate charges based on uncharged time only
3. Mark processed segments as charged

Example from `FreeMinutesRule`:
```csharp
public Task ApplyAsync(PricingContext context)
{
    if (context.CurrentDay == context.EntryTime.Date && freeMinutes > 0)
    {
        var freeEndTime = context.EntryTime.AddMinutes(freeMinutes);
        
        // Mark segments as charged with $0
        TimeSegmentHelper.MarkSegmentsAsCharged(
            context.TimeSegments,
            context.EntryTime,
            freeEndTime,
            Name,
            0);  // $0 charge
    }
    return Task.CompletedTask;
}
```

### 3. Segment Splitting

When a rule charges only part of a segment, it's automatically split:

**Before:**
```
[10:00 --------- 14:00] Uncharged, 240 minutes
```

**After marking 11:00-13:00 as charged:**
```
[10:00 - 11:00] Uncharged, 60 minutes
[11:00 - 13:00] Charged by HourlyRateRule, 120 minutes
[13:00 - 14:00] Uncharged, 60 minutes
```

### 4. Day-by-Day Processing

The engine processes each calendar day separately:
```csharp
foreach (var dayEntry in context.BillableMinutesPerDay.OrderBy(k => k.Key))
{
    context.CurrentDay = dayEntry.Key;
    context.CurrentDayBillableMinutes = context.GetUnchargedMinutesForCurrentDay();
    
    // Apply rules
    foreach (var rule in stageRules)
    {
        await rule.ApplyAsync(context);
        // Update uncharged minutes after each rule
        context.CurrentDayBillableMinutes = context.GetUnchargedMinutesForCurrentDay();
    }
}
```

## Updated Rules

All base calculation rules have been updated to be segment-aware:

### FreeMinutesRule
- Marks first N minutes as charged with $0
- Prevents subsequent rules from charging free time

### FreePeriodRule
- Marks entire days as free (charged with $0)
- Used for holidays or special free days

### HourlyRateRule
- Only charges **uncharged** segments
- Respects time-of-day pricing (different rates for different hours)
- Automatically skips already-charged time

### VariableRateRule
- Only charges **uncharged** segments
- Applies tiered pricing (e.g., first hour $5, next hour $7, etc.)
- Respects free periods

### DailyRateRule
- Checks if **uncharged** time >= threshold (23 hours)
- Charges flat daily rate if threshold met
- Marks all uncharged segments for the day

### ThresholdHourlyRule
- Applied on last day only
- If **uncharged** hours >= threshold, applies daily rate
- Useful for "round up to full day" policies

### EntryFeeRule, TaxRule, DiscountRule
- No changes needed (not time-based)

## Rule Execution Order

The order matters to prevent incorrect charging:

1. **PreProcess Stage**: Free periods and special rules
   - `FreePeriodRule` - Mark entire days as free
   - `FreeMinutesRule` - Mark initial minutes as free

2. **BaseCalculate Stage**: Core pricing (executed in this order!)
   - `EntryFeeRule` - Add fixed entry fee
   - `DailyRateRule` - Charge daily rate if applicable
   - `VariableRateRule` OR `HourlyRateRule` - Charge remaining time
   
3. **Adjust Stage**: Adjustments and thresholds
   - `ThresholdHourlyRule` - Round up to daily if close
   - `MultiplierRateRule` - Apply rate multipliers

4. **Finalize Stage**: Final calculations
   - `PercentageDiscountRule` - Apply discounts
   - `TaxRule` - Calculate and add tax

### Why Order Matters

**Critical**: `DailyRateRule` must run BEFORE `HourlyRateRule`!

**Wrong Order:**
```
1. HourlyRateRule charges all 24 hours → all segments marked as charged
2. DailyRateRule finds 0 uncharged segments → doesn't apply
Result: Charged hourly rate instead of cheaper daily rate!
```

**Correct Order:**
```
1. DailyRateRule finds 24 uncharged hours → charges flat daily rate
2. HourlyRateRule finds 0 uncharged segments → skips
Result: Correct daily rate applied
```

## Benefits

### ✅ No Double-Charging
Each time segment can only be charged once. Once marked as charged, it's excluded from all subsequent rules.

### ✅ Transparent Tracking
Every segment knows:
- Whether it's been charged
- Which rule charged it
- How much was charged

### ✅ Automatic Overlap Handling
When rules overlap partially, segments are automatically split to handle the overlap precisely.

### ✅ Flexible Rule Composition
Rules can be combined in any way without risk of double-charging:
- Free periods + hourly rates
- Daily rates + hourly fallback
- Multiple special rules

### ✅ Debug-Friendly
You can inspect `context.TimeSegments` to see exactly what happened:
```csharp
foreach (var segment in context.TimeSegments)
{
    Console.WriteLine(segment);
}
// Output:
// 2025-10-01 10:00 to 2025-10-01 11:00 (60 min) - Charged by Free Minutes (0)
// 2025-10-01 11:00 to 2025-10-02 00:00 (780 min) - Charged by Hourly Rate (130,000)
// ...
```

## Testing

### Unit Tests (`TimeSegmentTests.cs`)
- 10 tests covering TimeSegment and TimeSegmentHelper functionality
- Tests initialization, filtering, marking, splitting, and calculations

### Integration Tests (`PricingOverlapTests.cs`)
- 8 tests covering real-world overlap scenarios:
  - Free minutes + hourly rate
  - Free days + daily rate
  - Hourly + daily rate competition
  - Variable + hourly rate
  - Multi-day with free days
  - Threshold rules
  - Complex multi-rule scenarios

All tests pass ✅

## Migration Notes

### Breaking Changes
None - the changes are internal to the pricing engine.

### Backward Compatibility
- Existing rule interface (`IPricingRule`) unchanged
- All existing rules continue to work
- PricingContext extensions are additive

### Performance
- Minimal overhead: segment operations are O(n) where n = number of segments (typically 1-5)
- Segment splitting is only done when necessary
- Memory usage is negligible (few KB for typical parking durations)

## Future Enhancements

### Possible Improvements
1. **Time-of-Day Free Periods**: Free parking during specific hours (e.g., 11 PM - 6 AM)
2. **Segment Merging**: Combine adjacent segments with same charging status
3. **Partial Hour Pricing**: More granular minute-by-minute pricing
4. **Special Vehicle Types**: Different segment tracking for different vehicle types
5. **Capacity-Based Pricing**: Dynamic rates based on parking lot capacity

### Not Recommended
- **Sub-minute segments**: Current minute-based granularity is sufficient
- **Retroactive rule changes**: Would complicate the segment model significantly

## Conclusion

The time segment-based system provides a robust, flexible, and transparent solution for handling overlapping pricing rules in parking calculations. It ensures accurate pricing while maintaining code clarity and testability.
