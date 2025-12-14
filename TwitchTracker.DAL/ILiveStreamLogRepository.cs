using TwitchTracker.Models;

namespace TwitchTracker.DAL;

public interface ILiveStreamLogRepository
{
    Task AddSnapshotAsync(LiveStreamSnapshot snapshot);

    Task<List<LiveStreamSnapshot>> GetSnapshotsAsync(
        string streamerId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null);

    Task<List<string>> GetTrackedStreamersAsync();
}