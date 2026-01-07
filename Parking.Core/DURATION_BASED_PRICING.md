# Duration-Based Pricing System

## Overview

This document explains the duration-based pricing system that was added to address the fundamental issue with calendar-based parking fee calculations.

## Problem Statement

### The Core Issue

The original pricing system used **calendar day boundaries** (midnight to midnight) as the basis for calculations. This caused incorrect behavior when vehicles entered mid-day and stayed across calendar boundaries.

**Example Problem:**
- Entry: 10:12 on Day 1
- Exit: 18:48 on Day 2
- Total duration: ~32.5 hours (should be charged as 1 full day + ~8.5 hours)

**Calendar-Based Split:**
- Day 1: 10:12 to 23:59:59 = ~13.8 hours (828 minutes) ❌
- Day 2: 00:00:00 to 18:48 = ~18.8 hours (1128 minutes) ❌
- Neither day reaches the 23-hour threshold for daily rate!

**Duration-Based Split:**
- Period 1: 10:12 Day 1 to 10:12 Day 2 = 24 hours ✅
- Remaining: 10:12 Day 2 to 18:48 Day 2 = ~8.5 hours ✅
- Daily rate applies correctly!

## Solution Components

### 1. Enhanced SpecialRule Entity

Added properties to support long-term stay scenarios:

```csharp
public class SpecialRule
{
    // Existing properties...
    
    // NEW: Duration-based properties
    public int? MinDurationDays { get; set; }     // Minimum stay in days (e.g., 10)
    public int? MaxDurationDays { get; set; }     // Maximum stay in days (e.g., 20)
    public decimal? CustomDailyRate { get; set; } // Special rate for long-term stays
}
```

**Use Cases:**
- Configure different rates for different stay lengths
- Example: 1-7 days = $50/day, 8-30 days = $40/day, 31+ days = $30/day

### 2. Enhanced PricingContext

Added duration-based tracking alongside calendar-based tracking:

```csharp
public class PricingContext
{
    // EXISTING: Calendar-based (preserved for backward compatibility)
    public Dictionary<DateTime, int> BillableMinutesPerDay { get; set; } // Minutes per calendar day
    public DateTime CurrentDay { get; set; } // Current calendar day being processed
    
    // NEW: Duration-based
    public int TotalDurationDays { get; } // Full 24-hour periods from entry
    public int RemainingMinutesAfterFullDays { get; } // Minutes after full days
    
    // NEW: Duration-based query methods
    public int GetUnchargedMinutesInDurationPeriod(int periodIndex)
    public List<TimeSegment> GetUnchargedSegmentsInDurationPeriod(int periodIndex)
}
```

### 3. New Pricing Rules

#### DurationBasedDailyRateRule

Calculates daily rates based on full 24-hour periods from entry time.

**Constructor:**
```csharp
public DurationBasedDailyRateRule(
    decimal dailyRate,                    // Rate per full 24-hour period
    decimal? hourlyRateForRemaining = null, // Optional rate for partial days
    int minHoursForDailyRate = 23         // Minimum hours needed for daily rate
)
```

**Example Usage:**
```csharp
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
// Result: 140000 (1 day × 50000 + 9 hours × 10000)
```

**Key Features:**
- Calculates from vehicle entry time, not calendar midnight
- Handles multiple full days correctly
- Optional hourly rate for remaining time
- Configurable minimum hours threshold

#### LongTermStayRule

Applies custom rates for extended parking stays.

**Constructor:**
```csharp
public LongTermStayRule(
    int minDays,                          // Minimum days for this rate
    int maxDays,                          // Maximum days for this rate (0 = unlimited)
    decimal customDailyRate,              // Custom daily rate for this range
    decimal? hourlyRateForRemaining = null // Optional rate for partial days
)
```

**Example Usage:**
```csharp
var builder = new PricingPipelineBuilder();

// 10-20 day stays: $30/day (cheaper than regular $50/day)
builder.AddRule(PricingStage.BaseCalculate, 
    new LongTermStayRule(
        minDays: 10,
        maxDays: 20,
        customDailyRate: 30000,
        hourlyRateForRemaining: 5000));

// 21+ day stays: $20/day (even cheaper)
builder.AddRule(PricingStage.BaseCalculate, 
    new LongTermStayRule(
        minDays: 21,
        maxDays: 0, // 0 = unlimited
        customDailyRate: 20000,
        hourlyRateForRemaining: 5000));

var engine = builder.Build();

var context = new PricingContext
{
    EntryTime = new DateTime(2025, 10, 1, 10, 0, 0),
    ExitTime = new DateTime(2025, 10, 16, 15, 0, 0) // 15 days + 5 hours
};

var cost = await engine.CalculateAsync(context);
// Result: 475000 (15 days × 30000 + 5 hours × 5000)
```

**Key Features:**
- Supports multiple tier ranges
- Configurable min/max boundaries
- Priority-based (add most specific rules first)
- Handles partial days with optional hourly rate

## Usage Patterns

### Pattern 1: Simple Duration-Based Pricing

Replace calendar-based daily rate with duration-based:

```csharp
// OLD: Calendar-based (problematic)
builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));

// NEW: Duration-based (correct)
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, hourlyRateForRemaining: 10000));
```

### Pattern 2: Mixed Short-Term and Long-Term

Combine different rules for different durations:

```csharp
var builder = new PricingPipelineBuilder();

// Free first hour
builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(60));

// Short stays (< 10 days): Regular rate
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, 10000));

// Long stays (10-30 days): Discounted rate
builder.AddRule(PricingStage.BaseCalculate, 
    new LongTermStayRule(10, 30, 35000, 8000));

// Very long stays (30+ days): Heavily discounted
builder.AddRule(PricingStage.BaseCalculate, 
    new LongTermStayRule(31, 0, 25000, 5000));
```

### Pattern 3: Backward Compatible

Keep both systems for transition:

```csharp
var builder = new PricingPipelineBuilder();

// Keep calendar-based for legacy compatibility
builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));

// Add duration-based as alternative (will only charge uncharged time)
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, 10000));
```

## Migration Guide

### For Existing Systems

**Step 1:** Add the new properties to your SpecialRule database table:

```sql
ALTER TABLE SpecialRules ADD MinDurationDays INT NULL;
ALTER TABLE SpecialRules ADD MaxDurationDays INT NULL;
ALTER TABLE SpecialRules ADD CustomDailyRate DECIMAL(18,2) NULL;
```

**Step 2:** Test duration-based rules alongside existing rules:

```csharp
// Run both in parallel during testing
builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000)); // Old
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000)); // New

// Compare results, verify duration-based is correct
```

**Step 3:** Gradually migrate:

```csharp
// Option A: Replace completely
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, 10000));

// Option B: Keep both (duration-based will only charge uncharged segments)
builder.AddRule(PricingStage.BaseCalculate, new DailyRateRule(50000));
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, 10000));
```

### For New Systems

Start with duration-based from the beginning:

```csharp
var builder = new PricingPipelineBuilder();

// Entry fee
builder.AddRule(PricingStage.BaseCalculate, new EntryFeeRule(5000));

// Free first 30 minutes
builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(30));

// Duration-based daily rate
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000, 10000));

// Long-term rates
builder.AddRule(PricingStage.BaseCalculate, 
    new LongTermStayRule(10, 30, 35000, 8000));

// Tax
builder.AddRule(PricingStage.Finalize, new TaxRule(0.09m));
```

## Comparison: Calendar-Based vs Duration-Based

| Aspect | Calendar-Based | Duration-Based |
|--------|----------------|----------------|
| **Reference Point** | Midnight (00:00) | Vehicle entry time |
| **Day Definition** | Calendar day (midnight to midnight) | 24-hour period from entry |
| **Entry at 10:12, Exit Day+1 at 18:48** | 0 days charged (neither reaches threshold) | 1 day + 8.5 hours charged ✓ |
| **Entry at 14:00, Exit Day+2 at 20:00** | 0 days charged (no single day full) | 2 days + 6 hours charged ✓ |
| **Multi-day stays** | Undercounts days | Correct count ✓ |
| **Use Case** | Legacy systems | New implementations |

## Best Practices

### 1. Rule Ordering

Duration-based rules should be added in the correct stage:

```csharp
// ✓ CORRECT ORDER
builder.AddRule(PricingStage.PreProcess, new FreeMinutesRule(30));
builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));
builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));

// ✗ WRONG ORDER (hourly will charge everything before daily can apply)
builder.AddRule(PricingStage.BaseCalculate, new HourlyRateRule(segmentPrices));
builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));
```

### 2. Long-Term Rule Priority

Add most specific (longest) ranges first:

```csharp
// ✓ CORRECT: Most specific first
builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(31, 0, 20000));
builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(10, 30, 35000));
builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));

// ✗ WRONG: Short durations will match first
builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(10, 30, 35000));
builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(31, 0, 20000));
```

### 3. Testing

Always test edge cases:

```csharp
// Test: Exactly 24 hours
Entry: 2025-10-01 10:00, Exit: 2025-10-02 10:00 → 1 day

// Test: Just under 24 hours
Entry: 2025-10-01 10:00, Exit: 2025-10-02 09:59 → 0 days, 23.98 hours

// Test: Just over 24 hours
Entry: 2025-10-01 10:00, Exit: 2025-10-02 10:01 → 1 day, 1 minute

// Test: Multiple days with partial
Entry: 2025-10-01 14:00, Exit: 2025-10-04 20:00 → 3 days, 6 hours

// Test: Long-term boundary
Entry: 2025-10-01 10:00, Exit: 2025-10-10 10:00 → Exactly 9 days (not long-term)
Entry: 2025-10-01 10:00, Exit: 2025-10-11 10:00 → Exactly 10 days (long-term)
```

## Troubleshooting

### Issue: Daily rate not applying

**Symptom:** Entry at 10:00, exit at 09:00 next day (23 hours), no daily rate charged.

**Cause:** `minHoursForDailyRate` threshold not met (default is 23 hours exactly).

**Solution:**
```csharp
// Lower the threshold slightly
new DurationBasedDailyRateRule(50000, minHoursForDailyRate: 22)
```

### Issue: Long-term rate not applying

**Symptom:** 15-day stay not using long-term rate.

**Cause 1:** Duration calculation issue.
```csharp
// Check: Are you counting full 24-hour periods?
context.TotalDurationDays // Should be 15, not 14
```

**Cause 2:** Rule ordering.
```csharp
// Fix: Add long-term rule BEFORE regular daily rate
builder.AddRule(PricingStage.BaseCalculate, new LongTermStayRule(10, 20, 30000));
builder.AddRule(PricingStage.BaseCalculate, new DurationBasedDailyRateRule(50000));
```

### Issue: Double charging

**Symptom:** Both calendar and duration rules charging.

**Cause:** Both rule types added, overlapping.

**Solution:** Choose one approach:
```csharp
// Option 1: Only duration-based
builder.AddRule(PricingStage.BaseCalculate, 
    new DurationBasedDailyRateRule(50000));

// Option 2: Only calendar-based
builder.AddRule(PricingStage.BaseCalculate, 
    new DailyRateRule(50000));

// NOT BOTH (unless intentional)
```

## Summary

The duration-based pricing system provides accurate parking fee calculations based on the vehicle's actual entry time, fixing the fundamental issue with calendar-based calculations. It supports:

- ✅ Correct daily rate application regardless of entry time
- ✅ Multi-day stay handling
- ✅ Long-term stay custom rates
- ✅ Backward compatibility
- ✅ Flexible configuration
- ✅ Clear, testable behavior

For most new implementations, use `DurationBasedDailyRateRule` instead of `DailyRateRule` to ensure correct behavior.
