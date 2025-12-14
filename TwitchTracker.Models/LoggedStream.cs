namespace TwitchTracker.Models;

public class LoggedStream
{
    public string StreamId { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }

    public TimeSpan Duration => EndedAt - StartedAt;

    public long PeakViewers { get; set; }
    public long AverageViewers { get; set; }

    public string? GameName { get; set; }
    public string? Title { get; set; }

    public List<LiveStreamSnapshot> Snapshots { get; set; } = new();
}