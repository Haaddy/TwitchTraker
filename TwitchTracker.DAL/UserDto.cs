namespace TwitchTracker.DAL;

public class UserDto
{
    public string id {get;set;}
    public string username {get;set;}
    public string displayName {get;set;}
    
    public string profileImageUrl {get;set;}
    
    public bool isLive {get;set;}
}