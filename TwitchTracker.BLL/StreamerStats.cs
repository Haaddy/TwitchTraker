using TwitchTracker.DAL;
using TwitchTracker.Models;
using TwitchTracker.Services;

namespace TwitchTracker.BLL;

public class StreamerStats
{
    private readonly ITwitchServices _twitchServices;
    private readonly VODStreams _vodStreams;
    private readonly ILiveStreamLogRepository _logRepo;
    private readonly LiveStreamHistoryService _historyService;

    public StreamerStats(ITwitchServices twitchServices, ILiveStreamLogRepository logRepo)
    {
        _twitchServices = twitchServices;
        _vodStreams = new VODStreams(_twitchServices);
        _logRepo = logRepo;
        _historyService = new LiveStreamHistoryService(_logRepo);
    }

    public async Task<StreamerDto> GetStreamerInfoAsync(string login)
    {
        if (string.IsNullOrEmpty(login)) return null;
        return await _twitchServices.GetStreamerAsync(login);
    }

    public async Task<StreamDto> GetLiveStreamAsync(string login)
    {
        if (string.IsNullOrEmpty(login)) return null;
        return await _twitchServices.GetStreamAsync(login);
    }

    public async Task<List<EndStreamDto>> GetLastVodsAsync(string streamerId, int count = 50)
    {
        return await _vodStreams.GetLastStreamsAsync(streamerId, count);
    }

    //  Методы статистики по завершённым стримам 

    private async Task<List<LoggedStream>> GetLoggedStreamsAsync(string streamerId) //Получает список всех завершённых стримов стримера из истории
    {
        return await _historyService.GetStreamsAsync(streamerId);
    }

    public async Task<long> GetMaxViewersAsync(string streamerId, int lastNStreams = 0) //озвращает максимальный пиковый онлайн среди всех стримов или последних lastNStreams
    {
        var streams = await GetLoggedStreamsAsync(streamerId);
        if (lastNStreams > 0)
            streams = streams.OrderByDescending(s => s.StartedAt).Take(lastNStreams).ToList();

        return streams.Any() ? streams.Max(s => s.PeakViewers) : 0;
    }

    public async Task<long> GetAverageViewersAsync(string streamerId, int lastNStreams = 0) //Вычисляет средний онлайн за все или последние N стримов.
    {
        var streams = await GetLoggedStreamsAsync(streamerId);
        if (lastNStreams > 0)
            streams = streams.OrderByDescending(s => s.StartedAt).Take(lastNStreams).ToList();

        return streams.Any() ? (long)streams.Average(s => s.AverageViewers) : 0;
    }

    public async Task<TimeSpan> GetTotalDurationAsync(string streamerId, int lastNStreams = 0) //Считает суммарную длительность всех или последних N стримов
    {
        var streams = await GetLoggedStreamsAsync(streamerId);
        if (lastNStreams > 0)
            streams = streams.OrderByDescending(s => s.StartedAt).Take(lastNStreams).ToList();

        return streams.Aggregate(TimeSpan.Zero, (sum, s) => sum + s.Duration);
    }

    public async Task<int> GetStreamCountAsync(string streamerId, int lastNStreams = 0) //озвращает количество стримов всего или последних N.
    {
        var streams = await GetLoggedStreamsAsync(streamerId);
        if (lastNStreams > 0)
            streams = streams.OrderByDescending(s => s.StartedAt).Take(lastNStreams).ToList();

        return streams.Count;
    }
}
