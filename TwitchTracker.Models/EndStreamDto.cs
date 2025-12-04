namespace TwitchTracker.Models;

public class EndStreamDto
{
    // ID стрима или VOD
    public string StreamId { get; set; } = string.Empty;

    // ID стримера
    public string StreamerId { get; set; } = string.Empty;

    // Название стрима
    public string Title { get; set; } = string.Empty;

    // Название категории / игры
    public string GameName { get; set; } = string.Empty;

    // Время начала стрима
    // public DateTime StartedAt { get; set; }

    // Время окончания стрима
    // public DateTime EndedAt { get; set; }

    // Количество просмотров
    public long ViewCount { get; set; }

    // Ссылка на VOD (если есть)
    public string? Url { get; set; }

    // Превью стрима или VOD
    public string? ThumbnailUrl { get; set; }

    // Длительность стрима
    // public TimeSpan Duration => EndedAt - StartedAt;
    
}