using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TwitchTracker.DAL;
using TwitchTracker.BLL;

namespace TwitchTrackerUI
{
    public partial class GamePieChart : UserControl
    {
        private ILiveStreamLogRepository _logRepo;
        private LiveStreamHistoryService _historyService;
        private DispatcherTimer _timer;
        private string _streamerId;

        public GamePieChart()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += async (s, e) => await UpdateChartAsync();
        }

        /// <summary>
        /// Инициализация репозитория после создания через XAML
        /// </summary>
        public void Initialize(ILiveStreamLogRepository logRepo)
        {
            _logRepo = logRepo;
            _historyService = new LiveStreamHistoryService(_logRepo);
        }

        public void SetStreamer(string streamerId)
        {
            _streamerId = streamerId;
            _timer.Start();
            _ = UpdateChartAsync();
        }

        private async Task UpdateChartAsync()
        {
            if (_logRepo == null || string.IsNullOrEmpty(_streamerId))
                return;

            var streams = await _historyService.GetStreamsAsync(_streamerId);

            var gameStats = streams
                .Where(s => !string.IsNullOrEmpty(s.GameName))
                .GroupBy(s => s.GameName)
                .Select(g => new
                {
                    Game = g.Key,
                    AverageViewers = g.Average(s => s.AverageViewers),
                    StreamCount = g.Count()
                })
                .OrderByDescending(x => x.AverageViewers)
                .ToList();

            var plotModel = new PlotModel
            {
                Title = "Доля игр по онлайну",
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray,
                Background = OxyColors.DarkGray
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelPosition = 0.5,
                AngleSpan = 360,
                StartAngle = 0,
                TextColor = OxyColors.White,
                InsideLabelColor = OxyColors.White
            };

            foreach (var game in gameStats)
            {
                pieSeries.Slices.Add(new PieSlice(game.Game, game.AverageViewers)
                {
                    IsExploded = false,
                    Fill = OxyColor.FromRgb(
                        (byte)new Random(game.Game.GetHashCode()).Next(50, 200),
                        (byte)new Random(game.Game.GetHashCode() + 1).Next(50, 200),
                        (byte)new Random(game.Game.GetHashCode() + 2).Next(50, 200))
                });
            }

            plotModel.Series.Clear();
            plotModel.Series.Add(pieSeries);

            plotGamePie.Model = plotModel;
        }
    }
}
