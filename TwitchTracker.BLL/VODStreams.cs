using TwitchTracker.Models;
using TwitchTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchTracker.BLL
{
    public class VODStreams : ILastStreams
    {
        private readonly ITwitchServices _twitchServices;

        public List<EndStreamDto> Streams { get; private set; } = new List<EndStreamDto>();

        public VODStreams(ITwitchServices twitchServices)
        {
            _twitchServices = twitchServices;
        }

        // Получение последних VOD
        public async Task<List<EndStreamDto>> GetLastStreamsAsync(string streamerId, int count = 7)
        {
            var vodsFromApi = await _twitchServices.GetVodsAsync(streamerId, count);

            // Конвертируем Duration из строки Twitch ("1h2m30s") в TimeSpan
            Streams = vodsFromApi.Select(v => new EndStreamDto
            {
                StreamId = v.StreamId,
                StreamerId = v.StreamerId,
                Title = v.Title,
                Url = v.Url,
                ThumbnailUrl = v.ThumbnailUrl,
                ViewCount = v.ViewCount,
                StartedAtRaw = v.StartedAtRaw,
                DurationRaw = v.DurationRaw,
                StartedAt = DateTime.Parse(v.StartedAtRaw),
                Duration = ParseTwitchDuration(v.DurationRaw)
            }).ToList();

            return Streams;
        }

        // --- Методы расчета статистики за N дней ---

        public int GetStreamsCountForLastNDays(int n)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-n);
            return Streams.Count(v => v.StartedAt >= cutoffDate);
        }

        public TimeSpan GetAverageStreamDurationForLastNDays(int n)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-n);
            var filtered = Streams.Where(v => v.StartedAt >= cutoffDate).ToList();
            if (!filtered.Any()) return TimeSpan.Zero;

            var total = filtered.Aggregate(TimeSpan.Zero, (sum, v) => sum + v.Duration);
            return TimeSpan.FromSeconds(total.TotalSeconds / filtered.Count);
        }

        public long GetAverageViewsForLastNDays(int n)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-n);
            var filtered = Streams.Where(v => v.StartedAt >= cutoffDate).ToList();
            if (!filtered.Any()) return 0;

            return (long)filtered.Average(v => v.ViewCount);
        }

        // Парсинг формата Twitch (например "1h2m30s") в TimeSpan
        private TimeSpan ParseTwitchDuration(string twitchDuration)
        {
            int hours = 0, minutes = 0, seconds = 0;
            string number = "";

            foreach (char c in twitchDuration)
            {
                if (char.IsDigit(c))
                    number += c;
                else
                {
                    if (c == 'h') hours = int.Parse(number);
                    if (c == 'm') minutes = int.Parse(number);
                    if (c == 's') seconds = int.Parse(number);
                    number = "";
                }
            }

            return new TimeSpan(hours, minutes, seconds);
        }
    }
}
