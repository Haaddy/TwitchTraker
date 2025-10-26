using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchTracker.DAL;

public class TwitchService
{
    private readonly TwitchAPI _api; // Can use only in this class

    public TwitchService(string clientId, string clientSecret)
    {
        _api = new TwitchAPI();
        _api.Settings.ClientId = clientId;
        _api.Settings.AccessToken = clientSecret;
    }

    public async Task<UserDto> GetUserAsync(string login)
    {
        var response = await _api.Helix.Users.GetUsersAsync(logins: new(){login});
        var user = response.Users[0];

        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            DisplayName = user.DisplayName,
            ProfileImageUrl = user.ProfileImageUrl,
            ViewCount = user.ViewCount,
        };
    }
}