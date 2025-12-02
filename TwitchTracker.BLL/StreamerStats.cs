
using TwitchTracker.Services;
using TwitchTracker.Models;
namespace TwitchTracker.BLL;

public class StreamerStats
{
    public ITwitchServices TwitchServices { get; set; }

    public StreamerStats(ITwitchServices twitchServices)
    {
        TwitchServices = twitchServices;
        
    }

    public async Task<StreamerDto> GetStreamerInfoAsync(string login)
    {
        if (string.IsNullOrEmpty(login))
        {
            return null;
        }
        
        var streamerDto =  await TwitchServices.GetStreamerAsync(login);

        if (streamerDto == null)
        {
            return null;
        }
        
        return streamerDto;
    }

    public async Task<StreamDto> GetLiveStreamAsync(string login)
    {
        if (string.IsNullOrEmpty(login))
        {
            return null;
        }
        var streamDto = await TwitchServices.GetStreamAsync(login);

        if (streamDto == null)
        {
            return null;
        }
        
        return streamDto;
    }
    
}