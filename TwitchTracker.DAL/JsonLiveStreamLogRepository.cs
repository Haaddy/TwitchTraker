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

    public async Task AddSnapshotAsync(LiveStreamSnapshot snapshot)
    {
        // Создаём папку, если её нет
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

        // Сохраняем JSON с сохранением русских букв
        var output = JsonSerializer.Serialize(log, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        await File.WriteAllTextAsync(filePath, output);
    }

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

    public Task<List<string>> GetTrackedStreamersAsync()
    {
        if (!Directory.Exists(_basePath))
            return Task.FromResult(new List<string>());

        var list = Directory.GetFiles(_basePath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .ToList();

        return Task.FromResult(list);
    }
}
