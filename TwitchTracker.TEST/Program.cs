using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwitchTracker.BLL;
using TwitchTracker.DAL;
using TwitchTracker.Services;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // UTF-8 для русских символов (Rider / Windows)
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                string clientId = "ob1bnuwy4yzi5mgjz4f4n7b24z53np";
                string accessToken = "l5a0drbbczqyxk044zj6ipkkxz7jfl";

                // Services
                services.AddSingleton<ITwitchServices>(_ =>
                    new TwitchServices(clientId, accessToken));

                // DAL
                services.AddSingleton<ILiveStreamLogRepository, JsonLiveStreamLogRepository>();

                // BLL
                services.AddSingleton<TrackedStreamersService>();
                services.AddSingleton<StreamerStats>();
                services.AddSingleton<LiveStreamHistoryService>();
                services.AddSingleton<LiveStreamAnalytics>();

                // Background logging
                services.AddHostedService<LiveStreamLoggingService>();
            })
            .Build();

        // Получаем сервисы
        var trackedStreamers = host.Services.GetRequiredService<TrackedStreamersService>();
        var stats = host.Services.GetRequiredService<StreamerStats>();
        var history = host.Services.GetRequiredService<LiveStreamHistoryService>();
        var analytics = host.Services.GetRequiredService<LiveStreamAnalytics>();

        // Запуск фонового логирования
        await host.StartAsync();

        Console.WriteLine("TwitchTracker запущен.\n");

        while (true)
        {
            Console.Write("Введите ник стримера (Enter — выход): ");
            var login = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(login))
                break;

            // Добавляем в логирование (если уже есть — ничего страшного)
            trackedStreamers.Add(login);

            // === БАЗОВАЯ ИНФОРМАЦИЯ ===
            var streamer = await stats.GetStreamerInfoAsync(login);
            if (streamer == null)
            {
                Console.WriteLine("❌ Стример не найден.\n");
                continue;
            }

            Console.WriteLine($"\n=== {streamer.DisplayName} ({streamer.Login}) ===");
            Console.WriteLine($"ID: {streamer.StreamerId}");
            Console.WriteLine($"Подписчики: {streamer.TotalFollowers}");
            Console.WriteLine($"Био: {streamer.Bio}\n");

            // === ТЕКУЩИЙ СТРИМ ===
            var stream = await stats.GetLiveStreamAsync(login);
            if (stream.IsLive)
            {
                Console.WriteLine("🔴 Сейчас в эфире:");
                Console.WriteLine($"Тайтл: {stream.Title}");
                Console.WriteLine($"Зрители: {stream.ViewCount}");
                Console.WriteLine($"Категория: {stream.GameName}");
                Console.WriteLine($"Язык: {stream.Language}\n");
            }
            else
            {
                Console.WriteLine("⚫ Сейчас оффлайн\n");
            }

            // === VOD ===
            var vods = await stats.GetLastVodsAsync(streamer.StreamerId, 5);

            Console.WriteLine("📼 Последние VOD:");
            foreach (var vod in vods)
            {
                Console.WriteLine(
                    $"{vod.StartedAt:yyyy-MM-dd} | " +
                    $"{vod.ViewCount} просмотров | " +
                    $"{vod.Duration} | " +
                    $"{vod.Title}");
            }

            Console.WriteLine("\n📊 VOD-статистика (7 дней):");
            Console.WriteLine($"Стримов: {stats.GetStreamsCountForLastNDays(7)}");
            Console.WriteLine($"Средняя длительность: {stats.GetAverageStreamDurationForLastNDays(7)}");
            Console.WriteLine($"Средние просмотры: {stats.GetAverageViewsForLastNDays(7)}");

            // === ЛОГИ И АНАЛИТИКА ===
            var snapshots = await history.GetLiveSnapshotsAsync(streamer.StreamerId);
            var streams = await history.GetStreamsAsync(streamer.StreamerId);

            if (snapshots.Any())
            {
                Console.WriteLine("\n📈 Аналитика по логам:");
                Console.WriteLine($"Всего снапшотов: {snapshots.Count}");
                Console.WriteLine($"Пиковый онлайн: {analytics.GetPeakViewers(snapshots)}");
                Console.WriteLine($"Средний онлайн за стрим: {analytics.GetAverageViewersPerStream(streams)}");
                Console.WriteLine($"Средняя длительность стрима (по логам): {analytics.GetAverageStreamDuration(streams)}");

                Console.WriteLine("\n🕒 Последние снапшоты:");
                foreach (var s in snapshots.TakeLast(5))
                {
                    Console.WriteLine(
                        $"{s.TimestampUtc:HH:mm:ss} | " +
                        $"{s.Viewers} зрителей | " +
                        $"{s.GameName} | " +
                        $"{s.Title}");
                }
            }
            else
            {
                Console.WriteLine("\n⏳ Логи ещё не накоплены.");
            }

            Console.WriteLine("\n(Стример добавлен в фоновое логирование)\n");
        }

        await host.StopAsync();
        Console.WriteLine("Приложение остановлено.");
    }
}
