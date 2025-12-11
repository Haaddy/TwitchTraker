namespace TwitchTracker.Models;

public class StreamStatsDto
{
    public int TotalStreams { get; set; }
    public TimeSpan AvgDuration { get; set; }
    public double AvgViews { get; set; }

    public Dictionary<string, int> StreamsPerMonth { get; set; } = new();
}