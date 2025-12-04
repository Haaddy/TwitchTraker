using TwitchTracker.BLL;
using TwitchTracker.Services;

namespace TwitchTraker
{
    class Programh
    {
        public static async Task Main(string[] args)
        {

            ITwitchServices twitchServices = new TwitchServices("ob1bnuwy4yzi5mgjz4f4n7b24z53np","aw36654dlpjt2hddtco8eqvdvez1zv");
            StreamerStats stats = new StreamerStats(twitchServices,new VODStreams(twitchServices));
            
            while (true)
            {
                

                

               
                var info = await stats.GetStreamerInfoAsync("evelone2004");
                var liveStream = await stats.GetLiveStreamAsync("evelone2004");
                
                
                if (info == null)
                {
                    Console.WriteLine("Стример не найден.");
                    continue;
                }
                var vods = await stats.GetLastVodsAsync(info.StreamerId, 5);

                // вывод информации 
                Console.WriteLine("\n=== Информация о стримере ===");
                Console.WriteLine($"ID: {info.StreamerId}");
                Console.WriteLine($"Login: {info.Login}");
                Console.WriteLine($"Display Name: {info.DisplayName}");
                Console.WriteLine($"Avatar: {info.Avatar}");
                Console.WriteLine($"Followers: {info.TotalFollowers}");
                Console.WriteLine($"Description: {info.Bio}");
                
                Console.WriteLine($"Stream: {liveStream.Title}");
                Console.WriteLine($"Stream id: {liveStream.StreamerId}");
                Console.WriteLine($"Online: {liveStream.ViewCount}");
                Console.WriteLine($"Started: {liveStream.StartTime}");
                Console.WriteLine($"Stream is : {(liveStream.IsLive ? "online" : "offline")}");
                Console.WriteLine($"Stream: {liveStream.StreamerId}");
                Console.WriteLine($"cATEGORY: {liveStream.GameName}");
                Console.WriteLine($"Stream: {liveStream.GameId}");
                
                Console.WriteLine("\n=== Последние VOD ===");
                foreach (var vod in vods)
                {
                    Console.WriteLine($"Title: {vod.Title}");
                    Console.WriteLine($"StreamId: {vod.StreamId}");
                    Console.WriteLine($"StreamerId: {vod.StreamerId}");
                    Console.WriteLine($"Views: {vod.ViewCount}");
                    Console.WriteLine($"Url: {vod.Url}");
                    Console.WriteLine($"Thumbnail: {vod.ThumbnailUrl}");
                    Console.WriteLine("---------------------------");
                }
            }
        }
    }
}