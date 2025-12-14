using TwitchTracker.DAL;
using TwitchTracker.Models;

namespace TwitchTracker.BLL;

public class LiveStreamHistoryService
{
    private readonly ILiveStreamLogRepository _repo;

    public LiveStreamHistoryService(ILiveStreamLogRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<LiveStreamSnapshot>> GetLiveSnapshotsAsync(string streamerId)
    {
        return await _repo.GetSnapshotsAsync(streamerId);
    }

   public async Task<List<LoggedStream>> GetStreamsAsync(string streamerId)
{
    var snapshots = (await _repo.GetSnapshotsAsync(streamerId))
        .OrderBy(s => s.TimestampUtc)
        .ToList();

    var result = new List<LoggedStream>();
    LoggedStream? current = null;

    foreach (var s in snapshots)
    {
        if (s.IsLive)
        {
            if (current == null)
            {
                current = new LoggedStream
                {
                    StreamId = s.StreamId ?? Guid.NewGuid().ToString(),
                    StartedAt = s.TimestampUtc,
                    Title = s.Title,
                    GameName = s.GameName
                };
            }

            current.Snapshots.Add(s);
        }
        else if (current != null)
        {
            // Если нет промежуточных снэпшотов, добавим последний оффлайн-снэпшот для завершения
            if (!current.Snapshots.Any(snap => snap.TimestampUtc == s.TimestampUtc))
                current.Snapshots.Add(s);

            current.EndedAt = s.TimestampUtc;

            // Рассчитываем пиковый и средний онлайн
            if (current.Snapshots.Any())
            {
                current.PeakViewers = current.Snapshots.Max(snap => snap.Viewers);
                current.AverageViewers = (long)current.Snapshots.Average(snap => snap.Viewers);
            }
            else
            {
                current.PeakViewers = 0;
                current.AverageViewers = 0;
            }

            result.Add(current);
            current = null;
        }
    }

    // если стрим ещё идёт
    if (current != null)
    {
        current.EndedAt = DateTime.UtcNow;

        if (current.Snapshots.Any())
        {
            current.PeakViewers = current.Snapshots.Max(snap => snap.Viewers);
            current.AverageViewers = (long)current.Snapshots.Average(snap => snap.Viewers);
        }
        else
        {
            current.PeakViewers = 0;
            current.AverageViewers = 0;
        }

        result.Add(current);
    }

    return result;
}

    private void FinalizeStream(LoggedStream stream)
    {
        stream.PeakViewers = stream.Snapshots.Max(s => s.Viewers);
        stream.AverageViewers = (long)stream.Snapshots.Average(s => s.Viewers);
    }
}