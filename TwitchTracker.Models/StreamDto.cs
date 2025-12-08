namespace TwitchTracker.Models;

public class StreamDto
{
    public bool IsLive { get; set; } = false;
    public string? StreamId { get; set; } = null;
    public string? Title { get; set; } = null;
    public long? ViewCount { get; set; } = null;
    public DateTime? StartTime { get; set; } = null;
    public string? StreamerId { get; set; } = null;
    
    public string? GameId { get; set; } = null;
    public string? GameName { get; set; } = null;
}