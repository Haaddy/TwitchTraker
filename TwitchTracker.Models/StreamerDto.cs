namespace TwitchTracker.Models;

public class StreamerDto
{
    public string StreamerId { get; set; }
    public string Login { get; set; }
    public string DisplayName { get; set; }
    public string Avatar { get; set; } // url
    public string Bio { get; set; }
    public long TotalFollowers { get; set; }
    
}
