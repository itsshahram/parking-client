namespace Parking.Core.Models;

public class PricingContext
{
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public TimeSpan TotalDuration => ExitTime - EntryTime;
    public int TotalMinutes => (int)TotalDuration.TotalMinutes;

    public decimal BaseCost { get; set; } = 0;
    public decimal FinalCost { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;

    public List<string> AppliedRules { get; set; } = new();

    // Time segments for tracking charged/uncharged periods
    public List<TimeSegment> TimeSegments { get; set; } = new();
    
    // Duration-based properties for calculating full 24-hour periods from entry time
    public int TotalDurationDays => (int)TotalDuration.TotalDays; // Full 24-hour periods
    public int RemainingMinutesAfterFullDays => TotalMinutes - (TotalDurationDays * 1440); // Minutes after full days
    
    /// <summary>
    /// Gets uncharged minutes in a specific 24-hour period from entry time
    /// </summary>
    /// <param name="periodIndex">Zero-based index of 24-hour period (0 = first day, 1 = second day, etc.)</param>
    public int GetUnchargedMinutesInDurationPeriod(int periodIndex)
    {
        var periodStart = EntryTime.AddDays(periodIndex);
        var periodEnd = periodStart.AddDays(1);
        
        // Clamp to exit time if this is the last period
        if (periodEnd > ExitTime)
            periodEnd = ExitTime;
            
        return TimeSegmentHelper.GetUnchargedMinutesInRange(TimeSegments, periodStart, periodEnd);
    }
    
    /// <summary>
    /// Gets uncharged segments in a specific 24-hour period from entry time
    /// </summary>
    public List<TimeSegment> GetUnchargedSegmentsInDurationPeriod(int periodIndex)
    {
        var periodStart = EntryTime.AddDays(periodIndex);
        var periodEnd = periodStart.AddDays(1);
        
        if (periodEnd > ExitTime)
            periodEnd = ExitTime;
            
        return TimeSegmentHelper.FilterSegmentsByTimeRange(TimeSegments, periodStart, periodEnd, onlyUncharged: true);
    }
}