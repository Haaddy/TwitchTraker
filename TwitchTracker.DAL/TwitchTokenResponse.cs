namespace TwitchTracker.DAL;

public class TwitchTokenResponse
{
    public string access_token {get;set;}
    public int expires_in {get;set;}
    public string token_type {get;set;}
}