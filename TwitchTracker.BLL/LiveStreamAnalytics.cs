using TwitchTracker.Models;

namespace TwitchTracker.BLL;

public class LiveStreamAnalytics
{
    /// Пиковый онлайн за всё время
    public long GetPeakViewers(List<LiveStreamSnapshot> snapshots)
        => snapshots.Any() ? snapshots.Max(s => s.Viewers) : 0;

    /// Средний онлайн за стрим
    public long GetAverageViewersPerStream(
        Dictionary<string, List<LiveStreamSnapshot>> streams)
    {
        if (!streams.Any()) return 0;

        return (long)streams.Values
            .Average(stream =>
                stream.Average(s => s.Viewers));
    }

    /// Средняя длительность стрима (по логам)
    public TimeSpan GetAverageStreamDuration(
        Dictionary<string, List<LiveStreamSnapshot>> streams)
    {
        if (!streams.Any()) return TimeSpan.Zero;

        var durations = streams.Values.Select(stream =>
        {
            var start = stream.Min(s => s.TimestampUtc);
            var end = stream.Max(s => s.TimestampUtc);
            return end - start;
        });

        return TimeSpan.FromSeconds(
            durations.Average(d => d.TotalSeconds));
    }
    
    public List<ViewerPoint> GetViewerTimeline(List<LiveStreamSnapshot> snapshots)
    {
        return snapshots
            .OrderBy(s => s.TimestampUtc)
            .Select(s => new ViewerPoint
            {
                Time = s.TimestampUtc,
                Viewers = s.Viewers
            })
            .ToList();
    }
}