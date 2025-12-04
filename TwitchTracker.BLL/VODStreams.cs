using TwitchTracker.Models;
using TwitchTracker.Services;

namespace TwitchTracker.BLL;

public class VODStreams: ILastStreams
{
    
    private readonly ITwitchServices _twitchServices;
    
    public List<EndStreamDto> Streams { get; private set; } = new List<EndStreamDto>();

    public VODStreams(ITwitchServices twitchServices)
    {
        _twitchServices = twitchServices;
        
    }
    
    
    public async Task<List<EndStreamDto>> GetLastStreamsAsync(string streamerId, int count = 7)
    {
        Streams = await _twitchServices.GetVodsAsync(streamerId, count);
        return Streams;
    }
}