namespace Parking.Core.Models;

/// <summary>
/// Represents a continuous time segment with its pricing state.
/// Used to track which portions of parking time have been charged by which rules.
/// </summary>
public class TimeSegment
{
    /// <summary>
    /// Start time of this segment (inclusive)
    /// </summary>
    public DateTime Start { get; set; }
    
    /// <summary>
    /// End time of this segment (exclusive)
    /// </summary>
    public DateTime End { get; set; }
    
    /// <summary>
    /// Duration of this segment in minutes
    /// </summary>
    public int DurationMinutes => (int)(End - Start).TotalMinutes;
    
    /// <summary>
    /// Indicates whether this segment has been charged/processed by a rule
    /// </summary>
    public bool IsCharged { get; set; }
    
    /// <summary>
    /// Name of the rule that charged this segment (for tracking/debugging)
    /// </summary>
    public string? ChargedBy { get; set; }
    
    /// <summary>
    /// Amount charged for this specific segment
    /// </summary>
    public decimal ChargedAmount { get; set; }

    public TimeSegment(DateTime start, DateTime end, bool isCharged = false, string? chargedBy = null)
    {
        if (end <= start)
            throw new ArgumentException("End time must be after start time");
            
        Start = start;
        End = end;
        IsCharged = isCharged;
        ChargedBy = chargedBy;
    }

    /// <summary>
    /// Creates a copy of this segment
    /// </summary>
    public TimeSegment Clone()
    {
        return new TimeSegment(Start, End, IsCharged, ChargedBy)
        {
            ChargedAmount = ChargedAmount
        };
    }

    public override string ToString()
    {
        var status = IsCharged ? $"Charged by {ChargedBy} ({ChargedAmount:N0})" : "Uncharged";
        return $"{Start:yyyy-MM-dd HH:mm} to {End:yyyy-MM-dd HH:mm} ({DurationMinutes} min) - {status}";
    }
}
