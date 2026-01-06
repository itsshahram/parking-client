using Parking.Core.Models;
using Xunit;

namespace Parking.Test;

public class TimeSegmentTests
{
    [Fact]
    public void TimeSegment_Constructor_SetsPropertiesCorrectly()
    {
        // Arrange
        var start = new DateTime(2025, 10, 1, 10, 0, 0);
        var end = new DateTime(2025, 10, 1, 12, 0, 0);

        // Act
        var segment = new TimeSegment(start, end);

        // Assert
        Assert.Equal(start, segment.Start);
        Assert.Equal(end, segment.End);
        Assert.Equal(120, segment.DurationMinutes);
        Assert.False(segment.IsCharged);
        Assert.Null(segment.ChargedBy);
    }

    [Fact]
    public void TimeSegment_Constructor_ThrowsExceptionForInvalidRange()
    {
        // Arrange
        var start = new DateTime(2025, 10, 1, 12, 0, 0);
        var end = new DateTime(2025, 10, 1, 10, 0, 0);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TimeSegment(start, end));
    }

    [Fact]
    public void TimeSegmentHelper_InitializeSegments_CreatesCorrectSegments()
    {
        // Arrange
        var entryTime = new DateTime(2025, 10, 1, 10, 0, 0);
        var exitTime = new DateTime(2025, 10, 2, 14, 30, 0);

        // Act
        var segments = TimeSegmentHelper.InitializeSegments(entryTime, exitTime);

        // Assert
        Assert.Equal(2, segments.Count); // 2 days
        Assert.Equal(entryTime, segments[0].Start);
        Assert.Equal(new DateTime(2025, 10, 2, 0, 0, 0), segments[0].End);
        Assert.Equal(new DateTime(2025, 10, 2, 0, 0, 0), segments[1].Start);
        Assert.Equal(exitTime, segments[1].End);
    }

    [Fact]
    public void TimeSegmentHelper_GetUnchargedSegments_FiltersCorrectly()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 11, 0, 0), false),
            new TimeSegment(new DateTime(2025, 10, 1, 11, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0), true),
            new TimeSegment(new DateTime(2025, 10, 1, 12, 0, 0), new DateTime(2025, 10, 1, 13, 0, 0), false),
        };

        // Act
        var uncharged = TimeSegmentHelper.GetUnchargedSegments(segments);

        // Assert
        Assert.Equal(2, uncharged.Count);
        Assert.All(uncharged, s => Assert.False(s.IsCharged));
    }

    [Fact]
    public void TimeSegmentHelper_GetUnchargedMinutesForDay_CalculatesCorrectly()
    {
        // Arrange
        var day = new DateTime(2025, 10, 1);
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 11, 0, 0), false), // 60 min
            new TimeSegment(new DateTime(2025, 10, 1, 11, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0), true),  // 60 min charged
            new TimeSegment(new DateTime(2025, 10, 1, 12, 0, 0), new DateTime(2025, 10, 1, 14, 0, 0), false), // 120 min
        };

        // Act
        var minutes = TimeSegmentHelper.GetUnchargedMinutesForDay(segments, day);

        // Assert
        Assert.Equal(180, minutes); // 60 + 120
    }

    [Fact]
    public void TimeSegmentHelper_MarkSegmentsAsCharged_MarksCorrectly()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0), false),
        };

        // Act
        TimeSegmentHelper.MarkSegmentsAsCharged(
            segments,
            new DateTime(2025, 10, 1, 10, 0, 0),
            new DateTime(2025, 10, 1, 12, 0, 0),
            "TestRule",
            100);

        // Assert
        Assert.Single(segments);
        Assert.True(segments[0].IsCharged);
        Assert.Equal("TestRule", segments[0].ChargedBy);
        Assert.Equal(100, segments[0].ChargedAmount);
    }

    [Fact]
    public void TimeSegmentHelper_MarkSegmentsAsCharged_SplitsSegmentWhenPartialOverlap()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 14, 0, 0), false),
        };

        // Act - Mark only middle 2 hours as charged
        TimeSegmentHelper.MarkSegmentsAsCharged(
            segments,
            new DateTime(2025, 10, 1, 11, 0, 0),
            new DateTime(2025, 10, 1, 13, 0, 0),
            "TestRule",
            50);

        // Assert - Should have 3 segments: before, charged, after
        Assert.Equal(3, segments.Count);
        
        // First segment (uncharged)
        Assert.Equal(new DateTime(2025, 10, 1, 10, 0, 0), segments[0].Start);
        Assert.Equal(new DateTime(2025, 10, 1, 11, 0, 0), segments[0].End);
        Assert.False(segments[0].IsCharged);
        
        // Middle segment (charged)
        Assert.Equal(new DateTime(2025, 10, 1, 11, 0, 0), segments[1].Start);
        Assert.Equal(new DateTime(2025, 10, 1, 13, 0, 0), segments[1].End);
        Assert.True(segments[1].IsCharged);
        Assert.Equal("TestRule", segments[1].ChargedBy);
        
        // Last segment (uncharged)
        Assert.Equal(new DateTime(2025, 10, 1, 13, 0, 0), segments[2].Start);
        Assert.Equal(new DateTime(2025, 10, 1, 14, 0, 0), segments[2].End);
        Assert.False(segments[2].IsCharged);
    }

    [Fact]
    public void TimeSegmentHelper_MarkSegmentsAsCharged_HandlesMultipleSegments()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 11, 0, 0), false),
            new TimeSegment(new DateTime(2025, 10, 1, 11, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0), false),
            new TimeSegment(new DateTime(2025, 10, 1, 12, 0, 0), new DateTime(2025, 10, 1, 13, 0, 0), false),
        };

        // Act - Mark all segments as charged
        TimeSegmentHelper.MarkSegmentsAsCharged(
            segments,
            new DateTime(2025, 10, 1, 10, 0, 0),
            new DateTime(2025, 10, 1, 13, 0, 0),
            "TestRule",
            150);

        // Assert
        Assert.Equal(3, segments.Count);
        Assert.All(segments, s => Assert.True(s.IsCharged));
        Assert.All(segments, s => Assert.Equal("TestRule", s.ChargedBy));
    }

    [Fact]
    public void TimeSegmentHelper_GetTotalChargedAmount_CalculatesCorrectly()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 11, 0, 0), true, "Rule1")
            {
                ChargedAmount = 50
            },
            new TimeSegment(new DateTime(2025, 10, 1, 11, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0), true, "Rule2")
            {
                ChargedAmount = 75
            },
            new TimeSegment(new DateTime(2025, 10, 1, 12, 0, 0), new DateTime(2025, 10, 1, 13, 0, 0), false),
        };

        // Act
        var total = TimeSegmentHelper.GetTotalChargedAmount(segments);

        // Assert
        Assert.Equal(125, total);
    }

    [Fact]
    public void TimeSegmentHelper_GroupSegmentsByDay_GroupsCorrectly()
    {
        // Arrange
        var segments = new List<TimeSegment>
        {
            new TimeSegment(new DateTime(2025, 10, 1, 10, 0, 0), new DateTime(2025, 10, 1, 11, 0, 0)),
            new TimeSegment(new DateTime(2025, 10, 1, 11, 0, 0), new DateTime(2025, 10, 1, 12, 0, 0)),
            new TimeSegment(new DateTime(2025, 10, 2, 10, 0, 0), new DateTime(2025, 10, 2, 11, 0, 0)),
        };

        // Act
        var grouped = TimeSegmentHelper.GroupSegmentsByDay(segments);

        // Assert
        Assert.Equal(2, grouped.Count);
        Assert.Equal(2, grouped[new DateTime(2025, 10, 1)].Count);
        Assert.Single(grouped[new DateTime(2025, 10, 2)]);
    }
}
