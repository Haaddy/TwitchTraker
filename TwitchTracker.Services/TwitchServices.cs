using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchTracker.Models;

namespace TwitchTracker.Services;

public class TwitchServices : ITwitchServices
{
    private readonly TwitchAPI _twitchAPI;

    public TwitchServices(string clientId, string accessToken)
    {
        _twitchAPI = new TwitchAPI();
        _twitchAPI.Settings.ClientId = clientId;
        _twitchAPI.Settings.AccessToken = accessToken;
    }
    public async Task<long> GetTotalFollowersAsync(string id)
    {
        var result = await _twitchAPI.Helix.Channels.GetChannelFollowersAsync( broadcasterId: id);
        if (result == null) return 0;
        return result.Total;
        
    }
    
    public async Task<StreamerDto> GetStreamerAsync(string login)
    {
        var result = await _twitchAPI.Helix.Users.GetUsersAsync(logins: new List<string>() { login });
        var streamer = result.Users.FirstOrDefault();
        
        if (streamer == null)
        {
            return null;
        }
        string streamerId = streamer.Id;
        var totalFollower = await GetTotalFollowersAsync(streamerId);

        return new StreamerDto
        {
            StreamerId = streamerId,
            TotalFollowers = totalFollower,
            Login = streamer.Login,
            DisplayName = streamer.DisplayName,
            Bio = streamer.Description,
            Avatar = streamer.ProfileImageUrl,
        };
    }

    public async Task<StreamDto> GetStreamAsync(string login)
    {
        var result = await _twitchAPI.Helix.Streams.GetStreamsAsync(userLogins: new List<string> { login });
        var stream = result.Streams.FirstOrDefault();

        if (stream == null)
        {
            return new StreamDto { IsLive = false, StreamerId = login };
        }

        return new StreamDto
        {
            IsLive = true,
            StreamId = stream.Id,
            Title = stream.Title,
            ViewCount = stream.ViewerCount,
            StartTime = stream.StartedAt,
            StreamerId = stream.UserId,
            GameId = stream.GameId,
            GameName = stream.GameName,
            Language = stream.Language,
            
        };
    }
    
  
    public async Task<List<EndStreamDto>> GetVodsAsync(string streamerId, int count = 10)
    {
        var result = await _twitchAPI.Helix.Videos.GetVideosAsync(
            userId: streamerId,
            type: TwitchLib.Api.Core.Enums.VideoType.Archive,
            first: count
        );

        // Здесь только "сырые данные" в формате Twitch
        return result.Videos.Select(v => new EndStreamDto
        {
            StreamId = v.Id,
            StreamerId = v.UserId,
            Title = v.Title,
            StartedAtRaw = v.CreatedAt,   
            DurationRaw = v.Duration,     
            ViewCount = v.ViewCount,
            Url = v.Url,
            ThumbnailUrl = v.ThumbnailUrl,
            Language = v.Language,
        }).ToList();
    }
    
    
    


   
}
    