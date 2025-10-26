namespace TwitchTracker.BLL
{
    public class UserStats
    {
        public string DisplayName { get; private set; }
        public long ViewCount { get; private set; }
        public string PopularityLevel { get; private set; }

        public UserStats(DAL.UserDto dto)
        {
            DisplayName = dto.DisplayName;
            ViewCount = dto.ViewCount;

            // Простейшая логика оценки популярности
            PopularityLevel = ViewCount > 100000 ? "Very Popular" :
                ViewCount > 10000 ? "Popular" : "Newbie";
        }

        // Дополнительные методы для бизнес-логики
        public bool IsPopular() => ViewCount > 10000;
    }
}