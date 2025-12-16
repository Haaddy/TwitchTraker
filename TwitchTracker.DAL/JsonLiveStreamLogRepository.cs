using System.Text.Json;
using TwitchTracker.Models;

namespace TwitchTracker.DAL;

public class JsonLiveStreamLogRepository : ILiveStreamLogRepository
{
    private readonly string _basePath;

    public JsonLiveStreamLogRepository(string basePath = "Data/live")
    {
        _basePath = basePath;
    }

    public async Task AddSnapshotAsync(LiveStreamSnapshot snapshot) //Добавляет новый снимок стрима (LiveStreamSnapshot) в JSON-файл стримера.
    {
        Directory.CreateDirectory(_basePath);

        var filePath = Path.Combine(_basePath, $"{snapshot.Login}.json");

        LiveStreamerLog log;

        if (File.Exists(filePath))
        {
            var json = await File.ReadAllTextAsync(filePath);
            log = JsonSerializer.Deserialize<LiveStreamerLog>(json)
                  ?? new LiveStreamerLog();
        }
        else
        {
            log = new LiveStreamerLog
            {
                StreamerId = snapshot.StreamerId,
                Login = snapshot.Login
            };
        }

        log.Snapshots.Add(snapshot);

        var output = JsonSerializer.Serialize(log, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await File.WriteAllTextAsync(filePath, output);
    }

    //Получает все снимки стримов конкретного стримера
    public async Task<List<LiveStreamSnapshot>> GetSnapshotsAsync(
        string streamerId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        if (!Directory.Exists(_basePath))
            return new();

        foreach (var file in Directory.GetFiles(_basePath, "*.json"))
        {
            var json = await File.ReadAllTextAsync(file);
            var log = JsonSerializer.Deserialize<LiveStreamerLog>(json);

            if (log?.StreamerId != streamerId)
                continue;

            var result = log.Snapshots.AsEnumerable();

            if (fromUtc != null)
                result = result.Where(s => s.TimestampUtc >= fromUtc);

            if (toUtc != null)
                result = result.Where(s => s.TimestampUtc <= toUtc);

            return result.ToList();
        }

        return new();
    }

    // Получает все снимки стримов конкретного стримера по логину
    public async Task<List<LiveStreamSnapshot>> GetSnapshotsByLoginAsync(
        string login,
        DateTime? fromUtc = null,
        DateTime? toUtc = null)
    {
        if (!Directory.Exists(_basePath))
            return new();

        var filePath = Path.Combine(_basePath, $"{login}.json");
        if (!File.Exists(filePath))
            return new();

        var json = await File.ReadAllTextAsync(filePath);
        var log = JsonSerializer.Deserialize<LiveStreamerLog>(json);
        if (log == null) return new();

        var result = log.Snapshots.AsEnumerable();

        if (fromUtc != null)
            result = result.Where(s => s.TimestampUtc >= fromUtc);

        if (toUtc != null)
            result = result.Where(s => s.TimestampUtc <= toUtc);

        return result.ToList();
    }

    public Task<List<string>> GetTrackedStreamersAsync() //Возвращает список всех стримеров, за которыми ведётся логирование
    {
        if (!Directory.Exists(_basePath))
            return Task.FromResult(new List<string>());

        var list = Directory.GetFiles(_basePath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        return Task.FromResult(list);
    }

    public Task<IEnumerable<string>> GetAllStreamersWithLogsAsync()
    {
        if (!Directory.Exists(_basePath))
            return Task.FromResult(Enumerable.Empty<string>());

        var list = Directory.GetFiles(_basePath, "*.json")
            .Select(Path.GetFileNameWithoutExtension);

        return Task.FromResult(list);
    }

    
}
