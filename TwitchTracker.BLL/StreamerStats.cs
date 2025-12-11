using TwitchTracker.Services;
using TwitchTracker.Models;

namespace TwitchTracker.BLL;

public class StreamerStats
{
    private readonly ITwitchServices _twitchServices;
    private readonly VODStreams _vodStreams;

    public StreamerStats(ITwitchServices twitchServices)
    {
        _twitchServices = twitchServices;
        _vodStreams = new VODStreams(_twitchServices);
    }

    // Получение базовой информации о стримере
    public async Task<StreamerDto> GetStreamerInfoAsync(string login)
    {
        if (string.IsNullOrEmpty(login)) return null;
        return await _twitchServices.GetStreamerAsync(login);
    }

    // Получение текущего стрима
    public async Task<StreamDto> GetLiveStreamAsync(string login)
    {
        if (string.IsNullOrEmpty(login)) return null;
        return await _twitchServices.GetStreamAsync(login);
    }

    // Получение последних VOD
    public async Task<List<EndStreamDto>> GetLastVodsAsync(string streamerId, int count = 50)
    {
        return await _vodStreams.GetLastStreamsAsync(streamerId, count);
    }

    // --- Методы статистики за N дней ---
    public int GetStreamsCountForLastNDays(int n) => _vodStreams.GetStreamsCountForLastNDays(n);
    public TimeSpan GetAverageStreamDurationForLastNDays(int n) => _vodStreams.GetAverageStreamDurationForLastNDays(n);
    public long GetAverageViewsForLastNDays(int n) => _vodStreams.GetAverageViewsForLastNDays(n);
}
