using TwitchTracker.DAL;
using TwitchTracker.Models;

namespace TwitchTracker.BLL;

///! использует репозиторий ILiveStreamLogRepository для получения снэпшотов (логов онлайн-статистики стримов) и преобразует их в более удобные объекты LoggedStream, которые содержат информацию о каждом отдельном стриме
public class LiveStreamHistoryService
{
    private readonly ILiveStreamLogRepository _repo; // repository for log acssec

    public LiveStreamHistoryService(ILiveStreamLogRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<LiveStreamSnapshot>> GetLiveSnapshotsAsync(string streamerId) //Получает сырые снэпшоты по конкретному стримеру
    {
        return await _repo.GetSnapshotsAsync(streamerId);
    }

public async Task<List<LoggedStream>> GetStreamsAsync(string streamerId)
{
    // Получаем все снэпшоты стримера из репозитория и сортируем их по времени
    var snapshots = (await _repo.GetSnapshotsAsync(streamerId))
        .OrderBy(s => s.TimestampUtc)
        .ToList();

    // Список для хранения всех обработанных стримов
    var result = new List<LoggedStream>();
    // Текущий обрабатываемый стрим (если стрим идёт)
    LoggedStream? current = null;

    // Проходим по каждому снэпшоту
    foreach (var s in snapshots)
    {
        if (s.IsLive) // Если стрим активен
        {
            if (current == null) // Если текущий стрим ещё не создан
            {
                // Создаём новый объект LoggedStream и инициализируем базовые данные
                current = new LoggedStream
                {
                    StreamId = s.StreamId ?? Guid.NewGuid().ToString(), // используем StreamId или генерируем новый
                    StartedAt = s.TimestampUtc, // время старта стрима
                    Title = s.Title, // название стрима
                    GameName = s.GameName // игра на стриме
                };
            }

            // Добавляем снэпшот в текущий стрим
            current.Snapshots.Add(s);
        }
        else if (current != null) // Если стрим завершился (IsLive == false)
        {
            // Если нет снэпшота с текущим временем, добавляем его для завершения
            if (!current.Snapshots.Any(snap => snap.TimestampUtc == s.TimestampUtc))
                current.Snapshots.Add(s);

            // Устанавливаем время окончания стрима
            current.EndedAt = s.TimestampUtc;

            // Вычисляем пиковый и средний онлайн
            if (current.Snapshots.Any())
            {
                current.PeakViewers = current.Snapshots.Max(snap => snap.Viewers); // пиковый онлайн
                current.AverageViewers = (long)current.Snapshots.Average(snap => snap.Viewers); // средний онлайн
            }
            else
            {
                current.PeakViewers = 0;
                current.AverageViewers = 0;
            }

            // Добавляем завершённый стрим в результат
            result.Add(current);
            current = null; // сбрасываем текущий стрим
        }
    }

    // Если после цикла остался стрим, который ещё идёт
    if (current != null)
    {
        current.EndedAt = DateTime.UtcNow; // используем текущее время как завершение

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

        result.Add(current); // добавляем незавершённый стрим в результат
    }

    // Возвращаем список всех обработанных стримов
    return result;
}


    private void FinalizeStream(LoggedStream stream)
    {
        stream.PeakViewers = stream.Snapshots.Max(s => s.Viewers);
        stream.AverageViewers = (long)stream.Snapshots.Average(s => s.Viewers);
    }
}