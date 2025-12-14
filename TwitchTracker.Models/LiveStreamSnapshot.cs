namespace TwitchTracker.Models;

public class LiveStreamSnapshot
{
    public DateTime TimestampUtc { get; set; }

    public string StreamerId { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public bool IsLive { get; set; }

    public string? StreamId { get; set; }
    public string? Title { get; set; }

    public long Viewers { get; set; }

    public string? GameId { get; set; }
    public string? GameName { get; set; }

    public string? Language { get; set; }
}