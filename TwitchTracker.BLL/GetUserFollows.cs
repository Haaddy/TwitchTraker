using TwitchTracker.Models;
using TwitchTracker.Services;

namespace TwitchTracker.BLL;

public class GetUserFollows
{
    public ITwitchServices TwitchServices {get; set; }
    public List<FollowedChannelDto> FollowList { get; private set; } = new List<FollowedChannelDto>();

   public GetUserFollows(ITwitchServices _twitchServices)
    {
        TwitchServices = _twitchServices;
    }

    // public async Task<List<FollowedChannelDto>> GetUserFollowsAsync(string username)
    // {
    //     FollowList = await TwitchServices.GetFollowsAsync(username);
    //     if (FollowList == null)
    //     {
    //         Console.WriteLine("User does not exist.");
    //         return new List<FollowedChannelDto>();
    //     }
    //
    //     if (!FollowList.Any())
    //     {
    //         Console.WriteLine("No follows found.");
    //     }
    //     return FollowList;
    // }
}