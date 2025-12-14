using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TwitchTracker.BLL;
using TwitchTracker.DAL;

namespace TwitchTrackerUI
{
    public partial class HourlyViewersChart : UserControl
    {
        private ILiveStreamLogRepository _logRepo;
        private LiveStreamHistoryService _historyService;
        private DispatcherTimer _timer;
        private string _streamerId;

        public HourlyViewersChart()
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

            double[] avgViewersPerHour = new double[24];
            int[] countsPerHour = new int[24];

            foreach (var stream in streams)
            {
                foreach (var snap in stream.Snapshots)
                {
                    int hour = snap.TimestampUtc.Hour;
                    avgViewersPerHour[hour] += snap.Viewers;
                    countsPerHour[hour]++;
                }
            }

            for (int i = 0; i < 24; i++)
                if (countsPerHour[i] > 0)
                    avgViewersPerHour[i] /= countsPerHour[i];

            var plotModel = new PlotModel
            {
                Title = "",
                Background = OxyColor.FromRgb(24, 24, 27), // совпадает с основным окном
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray
            };

            // Оси
            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Средний онлайн",
                Minimum = 0,
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
                MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Час суток",
                TextColor = OxyColors.White,
                TitleColor = OxyColors.White,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColor.FromRgb(60, 60, 60),
                MinorGridlineColor = OxyColor.FromRgb(50, 50, 50),
                IsZoomEnabled = false,
                IsPanEnabled = false
            };
            categoryAxis.Labels.AddRange(Enumerable.Range(0, 24).Select(h => h.ToString()));
            plotModel.Axes.Add(categoryAxis);

            // Линия
            var lineSeries = new LineSeries
            {
                Color = OxyColor.FromRgb(145, 70, 255), // пурпурная линия
                MarkerType = MarkerType.Circle,
                MarkerSize = 5,
                MarkerFill = OxyColors.White,
                StrokeThickness = 2,
                TrackerFormatString = "Час: {2}\nОнлайн: {4:0}"
            };

            for (int i = 0; i < 24; i++)
                lineSeries.Points.Add(new DataPoint(i, Math.Round(avgViewersPerHour[i])));

            plotModel.Series.Add(lineSeries);

            plotHourlyViewers.Model = plotModel;
        }
    }
}
