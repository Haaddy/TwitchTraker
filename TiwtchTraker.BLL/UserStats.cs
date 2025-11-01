using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchTracker.DAL;


namespace TwitchTracker.BLL
{
    public class UserStats
    {
        public string username { get; set; }
        public string displayName { get; set; }
        public string profileImageUrl { get; set; }
        public bool isLive { get; set; }
        public UserStats(UserDto user)
        {
            username = user.username;
            displayName = user.displayName;
            profileImageUrl = user.profileImageUrl;
            isLive = user.isLive;
        }
    }
}