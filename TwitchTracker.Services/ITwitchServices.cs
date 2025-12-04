
using TwitchTracker.Models;

namespace TwitchTracker.Services;

public interface ITwitchServices
{
    Task<StreamerDto> GetStreamerAsync(string login);
    
    Task<StreamDto> GetStreamAsync(string login);
    
    Task<List<EndStreamDto>> GetVodsAsync(string streamerId, int count = 10);
}