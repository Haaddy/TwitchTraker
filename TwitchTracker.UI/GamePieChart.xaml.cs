using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;
using TwitchTracker.BLL;
using TwitchTracker.DAL;

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
                .SelectMany(s => s.Snapshots)
                .Where(s => !string.IsNullOrEmpty(s.GameName))
                .GroupBy(s => s.GameName)
                .Select(g => new
                {
                    Game = g.Key,
                    AverageViewers = g.Average(s => s.Viewers)
                })
                .OrderByDescending(x => x.AverageViewers)
                .ToList();

            var plotModel = new PlotModel
            {
                Title = "",
                Background = OxyColor.FromRgb(24, 24, 27), // совпадает с окном
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray
            };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1,
                InsideLabelPosition = 0.6,
                AngleSpan = 360,
                StartAngle = 0,
                TextColor = OxyColors.White,
                InsideLabelColor = OxyColors.White,
                TickHorizontalLength = 0,
                TickRadialLength = 0
            };

            foreach (var game in gameStats)
            {
                var random = new Random(game.Game.GetHashCode());
                var fill = OxyColor.FromRgb(
                    (byte)random.Next(145, 200),  // пурпурные оттенки
                    (byte)random.Next(50, 120),
                    (byte)random.Next(200, 255));

                pieSeries.Slices.Add(new PieSlice(game.Game, game.AverageViewers)
                {
                    Fill = fill,
                    IsExploded = false
                });
            }

            plotModel.Series.Clear();
            plotModel.Series.Add(pieSeries);

            plotGamePie.Model = plotModel;
        }
    }
}
