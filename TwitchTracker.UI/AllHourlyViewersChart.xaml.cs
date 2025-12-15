using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using TwitchTracker.DAL;

namespace TwitchTrackerUI
{
    public partial class AllHourlyViewersChart : UserControl
    {
        private ILiveStreamLogRepository _logRepo;
        private DispatcherTimer _timer;

        public AllHourlyViewersChart()
        {
            InitializeComponent();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
            _timer.Tick += async (s, e) => await UpdateChartAsync();
        }

        public void Initialize(ILiveStreamLogRepository logRepo)
        {
            _logRepo = logRepo;
            _ = UpdateChartAsync();
        }

        public void Start() => _timer.Start();

        private async Task UpdateChartAsync()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            if (_logRepo == null) return;

            // Получаем всех стримеров с логами по логину
            var logins = await _logRepo.GetAllStreamersWithLogsAsync();

            // --- Отладочный вывод ---
            Console.WriteLine($"Всего стримеров с логами: {logins.Count()}");
            foreach (var login in logins)
            {
                var snapshots = await _logRepo.GetSnapshotsByLoginAsync(login);
                Console.WriteLine($"Стример: {login}, снэпшотов: {snapshots.Count}");
            }
            // -----------------------

            if (!logins.Any())
            {
                // Пустой график, чтобы не ломался интерфейс
                plotAllHourlyViewers.Model = new PlotModel
                {
                    Title = "Средний онлайн по часам (Все стримеры)",
                    Background = OxyColor.FromRgb(24, 24, 27),
                    TextColor = OxyColors.White,
                    PlotAreaBorderColor = OxyColors.Gray
                };
                return;
            }

            var plotModel = new PlotModel
            {
                Title = "Средний онлайн по часам (Все стримеры)",
                Background = OxyColor.FromRgb(24, 24, 27),
                TextColor = OxyColors.White,
                PlotAreaBorderColor = OxyColors.Gray
            };

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
                IsZoomEnabled = true,
                IsPanEnabled = true
            });

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Час суток (UTC)",
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

            var random = new Random();

            foreach (var login in logins)
            {
                // Используем новый метод по логину
                var snapshots = await _logRepo.GetSnapshotsByLoginAsync(login);
                if (!snapshots.Any()) continue;

                double[] avgViewersPerHour = new double[24];
                int[] countsPerHour = new int[24];

                foreach (var snap in snapshots)
                {
                    int hour = snap.TimestampUtc.Hour;
                    avgViewersPerHour[hour] += snap.Viewers;
                    countsPerHour[hour]++;
                }

                for (int i = 0; i < 24; i++)
                    if (countsPerHour[i] > 0)
                        avgViewersPerHour[i] /= countsPerHour[i];

                var color = OxyColor.FromRgb(
                    (byte)random.Next(100, 255),
                    (byte)random.Next(100, 255),
                    (byte)random.Next(100, 255)
                );

                var lineSeries = new LineSeries
                {
                    Title = login,
                    Color = color,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3,
                    MarkerFill = color,
                    StrokeThickness = 2,
                    TrackerFormatString = "{0}\nЧас: {2}\nОнлайн: {4:0}"
                };

                for (int i = 0; i < 24; i++)
                    lineSeries.Points.Add(new DataPoint(i, avgViewersPerHour[i]));

                plotModel.Series.Add(lineSeries);
            }

            plotAllHourlyViewers.Model = plotModel;
        }
    }
}
