namespace TwitchTracker.Models;

public class LiveStreamerLog //представляет лог стримера.
{
    public string StreamerId { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;

    public List<LiveStreamSnapshot> Snapshots { get; set; } = new();
}