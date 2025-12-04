
using TwitchTracker.Services;
using TwitchTracker.Models;
namespace TwitchTracker.BLL;

public class StreamerStats
{
    public ITwitchServices TwitchServices { get; set; }
    private readonly ILastStreams _vodStreams;

    public StreamerStats(ITwitchServices twitchServices , ILastStreams vodStreams)
    {
        TwitchServices = twitchServices;
        _vodStreams = vodStreams;
        
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
    
    public async Task<List<EndStreamDto>> GetLastVodsAsync(string streamerId, int count = 7)
    {
        return await _vodStreams.GetLastStreamsAsync(streamerId, count);
    }
    
}