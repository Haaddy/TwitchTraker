using System.Threading.Tasks;
using TwitchTracker.BLL;
using TwitchTracker.DAL;

namespace TiwtchTraker.BLL;

public class UserManager
{
    private readonly TwitchService _twitchService;
    
    public UserManager(TwitchService twitchService)
    {
        _twitchService = twitchService;
        
        
    }

    public async Task<UserStats> GetUserStatsAsync(string login)
    {
        var userDto = await _twitchService.GetUserAsync(login);
        if (userDto == null)
        {
            return null;
        }

        return new UserStats(userDto);
    }
}