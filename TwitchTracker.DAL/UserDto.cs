namespace TwitchTracker.DAL;

public class UserDto
{
    public string Id {get; set;}
    public string DisplayName {get; set;}
    public string Login { get; set; }
    public string ProfileImageUrl { get; set; }
    public long ViewCount { get; set; }
}