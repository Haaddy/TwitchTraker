namespace TwitchTracker.Models;

public class FollowedChannelDto
{
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;

    public DateTime FollowedAt { get; set; }

    public string FollowerId { get; set; } = string.Empty;  // кто фоловит 
    public string FollowerName { get; set; } = string.Empty;
}