using Microsoft.Extensions.Hosting;
using TwitchTracker.DAL;
using TwitchTracker.Models;

namespace TwitchTracker.Services;

public class LiveStreamLoggingService : BackgroundService
{
    private readonly ITwitchServices _twitchServices;
    private readonly ILiveStreamLogRepository _repository;
    private readonly TrackedStreamersService _trackedStreamers;

    private readonly TimeSpan _interval = TimeSpan.FromSeconds(60);

    public LiveStreamLoggingService(
        ITwitchServices twitchServices,
        ILiveStreamLogRepository repository,
        TrackedStreamersService trackedStreamers)
    {
        _twitchServices = twitchServices;
        _repository = repository;
        _trackedStreamers = trackedStreamers;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var login in _trackedStreamers.Streamers)
            {
                try
                {
                    await LogStreamerAsync(login);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOG ERROR] {login}: {ex.Message}");
                }
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task LogStreamerAsync(string login)
    {
        var streamer = await _twitchServices.GetStreamerAsync(login);
        if (streamer == null) return;

        var stream = await _twitchServices.GetStreamAsync(login);

        // Получаем последний снэпшот для сравнения
        var snapshots = await _repository.GetSnapshotsAsync(streamer.StreamerId);
        var lastSnapshot = snapshots.OrderByDescending(s => s.TimestampUtc).FirstOrDefault();

        // Создаём новый снэпшот
        var snapshot = new LiveStreamSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            StreamerId = streamer.StreamerId,
            Login = login,
            IsLive = stream.IsLive,
            StreamId = stream.StreamId,
            Title = stream.Title,
            Viewers = stream.ViewCount ?? 0,
            GameId = stream.GameId,
            GameName = stream.GameName,
            Language = stream.Language
        };

        // Условие 1: если оффлайн и последний снэпшот тоже оффлайн → пропускаем
        if (!snapshot.IsLive && lastSnapshot?.IsLive == false)
            return;

        // Условие 2: если данные не изменились → пропускаем
        if (lastSnapshot != null && AreSnapshotsEqual(lastSnapshot, snapshot))
            return;

        await _repository.AddSnapshotAsync(snapshot);
    }

    private bool AreSnapshotsEqual(LiveStreamSnapshot oldSnap, LiveStreamSnapshot newSnap)
    {
        return oldSnap.IsLive == newSnap.IsLive
               && oldSnap.Viewers == newSnap.Viewers
               && oldSnap.Title == newSnap.Title
               && oldSnap.GameId == newSnap.GameId
               && oldSnap.Language == newSnap.Language;
    }
}
