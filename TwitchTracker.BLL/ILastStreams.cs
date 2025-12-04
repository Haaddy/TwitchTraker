using TwitchTracker.Models;

namespace TwitchTracker.BLL;

public interface ILastStreams
{
    Task<List<EndStreamDto>> GetLastStreamsAsync(string streamerId,int count = 7);
}