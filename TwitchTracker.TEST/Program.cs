using TwitchTracker.BLL;
using TwitchTracker.Services;

namespace TwitchTraker
{
    class Programh
    {
        public static async Task Main(string[] args)
        {

            ITwitchServices twitchServices = new TwitchServices(
                "ob1bnuwy4yzi5mgjz4f4n7b24z53np",
                "rbrzchwehbvjqros53sgbvjwm07v1e"
            );

            StreamerStats stats = new StreamerStats(twitchServices);

            var streamerInfo = await stats.GetStreamerInfoAsync("evelone2004");
            var liveStream = await stats.GetLiveStreamAsync("evelone2004");

            Console.WriteLine("\n=== Streamer info ===");
            Console.WriteLine($"ID: {streamerInfo.StreamerId}");
            Console.WriteLine($"Login: {streamerInfo.Login}");
            Console.WriteLine($"Display Name: {streamerInfo.DisplayName}");
            Console.WriteLine($"Avatar: {streamerInfo.Avatar}");
            Console.WriteLine($"Followers: {streamerInfo.TotalFollowers}");
            Console.WriteLine($"Description: {streamerInfo.Bio}");

            Console.WriteLine("\n=== Current stream ===");
            if (liveStream.IsLive)
            {
                Console.WriteLine($"Stream: {liveStream.Title}");
                Console.WriteLine($"Stream ID: {liveStream.StreamId}");
                Console.WriteLine($"Viewers: {liveStream.ViewCount}");
                Console.WriteLine($"Started: {liveStream.StartTime}");
                Console.WriteLine($"Category: {liveStream.GameName}");
            }
            else
            {
                Console.WriteLine("offline");
            }

            // Получаем VOD
            var vods = await stats.GetLastVodsAsync(streamerInfo.StreamerId, 15);
            Console.WriteLine("\n=== Lasted 10 VOD ===");
            foreach (var vod in vods)
            {
                Console.WriteLine($"Title: {vod.Title}");
                Console.WriteLine($"Started: {vod.StartedAt}");
                Console.WriteLine($"Duration: {vod.Duration}");
                Console.WriteLine($"Views: {vod.ViewCount}");
                Console.WriteLine($"URL: {vod.Url}");
                Console.WriteLine("------------------------");
            }

            // Статистика за последние N дней (например 30)
            int days = 30;
            Console.WriteLine($"\n=== Stats for last  {days} дней ===");
            Console.WriteLine($"Count of Streams: {stats.GetStreamsCountForLastNDays(days)}");
            Console.WriteLine($"AVG Duration: {stats.GetAverageStreamDurationForLastNDays(days)}");
            Console.WriteLine($"AVG Views: {stats.GetAverageViewsForLastNDays(days)}");
        }


    }
}
