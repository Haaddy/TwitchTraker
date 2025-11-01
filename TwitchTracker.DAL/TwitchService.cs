using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using System.Net.Http;
using System.Net.Http.Headers; 
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api.ThirdParty.AuthorizationFlow;

namespace TwitchTracker.DAL;

public class TwitchService
{
    private readonly TwitchAPI _api;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken;
    private DateTime _accessTokenExpiration;
    
    
    public TwitchService(string clientId, string clientSecret, string accessToken)
    {
        _clientId = clientId;
        _clientSecret = clientSecret;
        _accessToken = accessToken;
        _api = new TwitchAPI();
        Console.WriteLine(_accessToken);
        
    }

    public async Task EsureTokenValidAsync()
    {
        var valid = await _api.Auth.ValidateAccessTokenAsync();
        if (valid == null)
        {
            await RefreshTokenAsync();
            _api.Settings.AccessToken = _accessToken;
            _api.Settings.ClientId = _clientId;
            Console.WriteLine(_accessToken);
        }
        
    }

    private async Task RefreshTokenAsync()
    {
        using var http = new HttpClient();

        var response = await http.PostAsync($"https://id.twitch.tv/oauth2/token?client_id={_clientId}&client_secret={_clientSecret}&grant_type=client_credentials", null);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine(json);
        

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var tokenData = JsonSerializer.Deserialize<TwitchTokenResponse>(json, options);
        _accessToken = tokenData.access_token;
        _accessTokenExpiration = DateTime.UtcNow.AddSeconds(tokenData.expires_in);
        
    }

    public async Task<bool> isLiveAsync(string login)
    {
        
        await EsureTokenValidAsync();
        var result = await _api.Helix.Streams.GetStreamsAsync(userLogins: new List<string> {login});
        
        return result.Streams != null && result.Streams.Any();
    }
    public async Task<UserDto> GetUserAsync(String login)
    {
        await EsureTokenValidAsync();
        
        var result = await _api.Helix.Users.GetUsersAsync(logins: new List<string> {login});

        var user = result.Users.FirstOrDefault();
        var isLive = await isLiveAsync(login);
        
        if (user == null)
        {
            return null;
        }

        return new UserDto
        {
            id = user.Id,
            username = user.Login,
            displayName = user.DisplayName,
            profileImageUrl = user.ProfileImageUrl,
            isLive = isLive,
            
        };



    }
}