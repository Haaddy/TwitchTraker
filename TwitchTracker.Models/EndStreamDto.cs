namespace TwitchTracker.Models;

public class EndStreamDto
{
    public string StreamId { get; set; } = string.Empty;
    public string StreamerId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // Строковое значение от API (для хранения "сырых данных")
    public string StartedAtRaw { get; set; } = string.Empty;
    public string DurationRaw { get; set; } = string.Empty;

    // Для расчётов в BLL (TimeSpan / DateTime)
    public DateTime StartedAt { get; set; }
    public TimeSpan Duration { get; set; }

    public long ViewCount { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
}