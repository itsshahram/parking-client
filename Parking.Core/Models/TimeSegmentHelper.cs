namespace Parking.Core.Models;

/// <summary>
/// Helper class for time segment operations
/// </summary>
public static class TimeSegmentHelper
{
    /// <summary>
    /// Gets all uncharged segments from a list
    /// </summary>
    public static List<TimeSegment> GetUnchargedSegments(List<TimeSegment> segments)
    {
        return segments.Where(s => !s.IsCharged).ToList();
    }
    
    /// <summary>
    /// Gets all uncharged segments for a specific day
    /// </summary>
    public static List<TimeSegment> GetUnchargedSegmentsForDay(List<TimeSegment> segments, DateTime day)
    {
        var dayStart = day.Date;
        var dayEnd = dayStart.AddDays(1);
        
        return segments
            .Where(s => !s.IsCharged && s.Start >= dayStart && s.Start < dayEnd)
            .ToList();
    }
    
    /// <summary>
    /// Calculates total uncharged minutes for a specific day
    /// </summary>
    public static int GetUnchargedMinutesForDay(List<TimeSegment> segments, DateTime day)
    {
        return GetUnchargedSegmentsForDay(segments, day)
            .Sum(s => s.DurationMinutes);
    }
    
    /// <summary>
    /// Calculates total uncharged minutes within a specific time range
    /// </summary>
    public static int GetUnchargedMinutesInRange(List<TimeSegment> segments, DateTime start, DateTime end)
    {
        return segments
            .Where(s => !s.IsCharged && s.Start < end && s.End > start)
            .Sum(s => 
            {
                // Calculate overlap between segment and range
                var overlapStart = s.Start > start ? s.Start : start;
                var overlapEnd = s.End < end ? s.End : end;
                return (int)(overlapEnd - overlapStart).TotalMinutes;
            });
    }
    
    /// <summary>
    /// Marks segments as charged within a specific time range.
    /// The 'amount' parameter represents the total charge for the entire time range,
    /// which will be distributed proportionally across all affected segments.
    /// </summary>
    public static void MarkSegmentsAsCharged(
        List<TimeSegment> segments,
        DateTime start,
        DateTime end,
        string chargedBy,
        decimal amount = 0)
    {
        var affectedSegments = segments
            .Where(s => !s.IsCharged && s.Start < end && s.End > start)
            .ToList();
        
        if (affectedSegments.Count == 0)
            return;
        
        // Calculate total minutes to be charged for proportional distribution
        var totalMinutesToCharge = affectedSegments.Sum(s =>
        {
            var overlapStart = s.Start > start ? s.Start : start;
            var overlapEnd = s.End < end ? s.End : end;
            return (int)(overlapEnd - overlapStart).TotalMinutes;
        });
        
        foreach (var segment in affectedSegments)
        {
            // If segment is completely within the range, just mark it
            if (segment.Start >= start && segment.End <= end)
            {
                // Distribute amount proportionally based on segment duration
                var proportionalAmount = totalMinutesToCharge > 0
                    ? amount * segment.DurationMinutes / totalMinutesToCharge
                    : 0;
                
                segment.IsCharged = true;
                segment.ChargedBy = chargedBy;
                segment.ChargedAmount = proportionalAmount;
            }
            else
            {
                // Need to split the segment
                var overlapStart = segment.Start > start ? segment.Start : start;
                var overlapEnd = segment.End < end ? segment.End : end;
                var overlapMinutes = (int)(overlapEnd - overlapStart).TotalMinutes;
                var proportionalAmount = totalMinutesToCharge > 0
                    ? amount * overlapMinutes / totalMinutesToCharge
                    : 0;
                
                var splitSegments = SplitSegment(segment, start, end, chargedBy, proportionalAmount);
                
                // Remove original segment
                segments.Remove(segment);
                
                // Add split segments
                segments.AddRange(splitSegments);
            }
        }
    }
    
    /// <summary>
    /// Marks specific segments as charged.
    /// NOTE: The totalAmount is distributed equally across all segments.
    /// For more accurate distribution, calculate per-segment amounts before calling this method.
    /// </summary>
    public static void MarkSegmentsAsCharged(
        List<TimeSegment> segments,
        List<TimeSegment> segmentsToCharge,
        string chargedBy,
        decimal totalAmount = 0)
    {
        var amountPerSegment = segmentsToCharge.Count > 0 
            ? totalAmount / segmentsToCharge.Count 
            : 0;
        
        foreach (var segmentToCharge in segmentsToCharge)
        {
            var matchingSegment = segments.FirstOrDefault(s => 
                s.Start == segmentToCharge.Start && 
                s.End == segmentToCharge.End && 
                !s.IsCharged);
            
            if (matchingSegment != null)
            {
                matchingSegment.IsCharged = true;
                matchingSegment.ChargedBy = chargedBy;
                matchingSegment.ChargedAmount = amountPerSegment;
            }
        }
    }
    
    /// <summary>
    /// Splits a segment by a time range, creating up to 3 segments:
    /// - Before the range (uncharged)
    /// - Within the range (charged)
    /// - After the range (uncharged)
    /// </summary>
    private static List<TimeSegment> SplitSegment(
        TimeSegment segment,
        DateTime splitStart,
        DateTime splitEnd,
        string chargedBy,
        decimal amount)
    {
        var result = new List<TimeSegment>();
        
        // Part before the split range
        if (segment.Start < splitStart)
        {
            result.Add(new TimeSegment(segment.Start, splitStart, false, null));
        }
        
        // Part within the split range (charged)
        var chargedStart = segment.Start > splitStart ? segment.Start : splitStart;
        var chargedEnd = segment.End < splitEnd ? segment.End : splitEnd;
        
        if (chargedStart < chargedEnd)
        {
            result.Add(new TimeSegment(chargedStart, chargedEnd, true, chargedBy)
            {
                ChargedAmount = amount
            });
        }
        
        // Part after the split range
        if (segment.End > splitEnd)
        {
            result.Add(new TimeSegment(splitEnd, segment.End, false, null));
        }
        
        return result;
    }
    
    /// <summary>
    /// Initializes segments from entry to exit time
    /// </summary>
    public static List<TimeSegment> InitializeSegments(DateTime entryTime, DateTime exitTime)
    {
        var segments = new List<TimeSegment>();
        var current = entryTime;
        
        while (current < exitTime)
        {
            // Create segments per day for easier day-based processing
            var dayEnd = current.Date.AddDays(1);
            var segmentEnd = dayEnd < exitTime ? dayEnd : exitTime;
            
            segments.Add(new TimeSegment(current, segmentEnd, false, null));
            current = segmentEnd;
        }
        
        return segments;
    }
    
    /// <summary>
    /// Filters segments that intersect with a specific time range
    /// </summary>
    public static List<TimeSegment> FilterSegmentsByTimeRange(
        List<TimeSegment> segments,
        DateTime start,
        DateTime end,
        bool onlyUncharged = true)
    {
        return segments
            .Where(s => s.Start < end && s.End > start)
            .Where(s => !onlyUncharged || !s.IsCharged)
            .ToList();
    }
    
    /// <summary>
    /// Gets segments within a specific time of day range (e.g., 9:00 AM to 5:00 PM)
    /// </summary>
    public static List<TimeSegment> FilterSegmentsByTimeOfDay(
        List<TimeSegment> segments,
        TimeSpan timeFrom,
        TimeSpan timeTo,
        bool onlyUncharged = true)
    {
        var result = new List<TimeSegment>();
        
        foreach (var segment in segments)
        {
            if (onlyUncharged && segment.IsCharged)
                continue;
            
            var segmentStartTime = segment.Start.TimeOfDay;
            var segmentEndTime = segment.End.TimeOfDay;
            
            // Handle midnight crossing
            if (timeTo < timeFrom)
            {
                // Range crosses midnight (e.g., 22:00 to 06:00)
                if (segmentStartTime >= timeFrom || segmentEndTime <= timeTo)
                {
                    result.Add(segment);
                }
            }
            else
            {
                // Normal range (e.g., 09:00 to 17:00)
                if (segmentStartTime >= timeFrom && segmentStartTime < timeTo)
                {
                    result.Add(segment);
                }
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculates total charged amount across all segments
    /// </summary>
    public static decimal GetTotalChargedAmount(List<TimeSegment> segments)
    {
        return segments.Where(s => s.IsCharged).Sum(s => s.ChargedAmount);
    }
    
    /// <summary>
    /// Groups segments by day
    /// </summary>
    public static Dictionary<DateTime, List<TimeSegment>> GroupSegmentsByDay(List<TimeSegment> segments)
    {
        return segments
            .GroupBy(s => s.Start.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
