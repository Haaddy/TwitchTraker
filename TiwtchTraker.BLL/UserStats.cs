using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchTracker.DAL;


namespace TwitchTracker.BLL
{
    public class UserStats
    {
        public string username { get; set; }

        public UserStats(UserDto user)
        {
            username = user.username;
        }
    }
}