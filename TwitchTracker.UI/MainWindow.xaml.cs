using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwitchTracker.BLL;
using TwitchTracker.DAL;
using TwitchTracker.Models;
using TwitchTracker.Services;

namespace TwitchTrackerUI
{
    public partial class MainWindow : Window
    {
        private readonly StreamerStats _stats;
        private readonly TrackedStreamersService _tracked;
        private readonly ILiveStreamLogRepository _logRepo;

        private DispatcherTimer _updateTimer;
        private string _currentLogin;

        public MainWindow()
        {
            InitializeComponent();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<ITwitchServices>(_ => new TwitchServices(
                        "ob1bnuwy4yzi5mgjz4f4n7b24z53np",
                        "t767s7glzpa47c5sfuldxw5qbl89oh"));

                    services.AddSingleton<ILiveStreamLogRepository, JsonLiveStreamLogRepository>();
                    services.AddSingleton<TrackedStreamersService>();
                    services.AddSingleton<StreamerStats>();
                    services.AddHostedService<LiveStreamLoggingService>();
                })
                .Build();

            host.Start();

            _stats = host.Services.GetRequiredService<StreamerStats>();
            _tracked = host.Services.GetRequiredService<TrackedStreamersService>();
            _logRepo = host.Services.GetRequiredService<ILiveStreamLogRepository>();

            btnSearch.Click += BtnSearch_Click;

            // Таймер для автообновления метрик и текущего стрима
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _updateTimer.Tick += async (s, e) => await UpdateMetricsAsync();
            _updateTimer.Start();
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string login = txtStreamerLogin.Text.Trim();
            if (string.IsNullOrEmpty(login)) return;

            _currentLogin = login;
            _tracked.Add(login);

            await UpdateMetricsAsync();
        }

        private async Task UpdateMetricsAsync()
        {
            if (string.IsNullOrEmpty(_currentLogin)) return;

            var streamer = await _stats.GetStreamerInfoAsync(_currentLogin);
            if (streamer == null)
            {
                MessageBox.Show("Стример не найден.");
                return;
            }

            spStreamerInfo.Visibility = Visibility.Visible;

            imgAvatar.Source = new BitmapImage(new Uri(streamer.Avatar));
            txtDisplayName.Text = streamer.DisplayName;
            txtFollowers.Text = $"Подписчики: {streamer.TotalFollowers}";
            txtBio.Text = streamer.Bio;

            var stream = await _stats.GetLiveStreamAsync(_currentLogin);
            UpdateLiveStreamUI(stream);

            await UpdateCompletedStreamStatsAsync(streamer.StreamerId);
        }

        private void UpdateLiveStreamUI(StreamDto stream)
        {
            if (stream == null || !stream.IsLive)
            {
                brdLiveStream.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                ellipseLive.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                txtStreamStatus.Text = "Оффлайн";
                txtStreamTitle.Text = "";
                txtStreamViewers.Text = "";
                txtStreamGame.Text = "";
                txtStreamLanguage.Text = "";
            }
            else
            {
                brdLiveStream.Background = new SolidColorBrush(Color.FromRgb(145, 70, 255));
                ellipseLive.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                txtStreamStatus.Text = "В эфире";
                txtStreamTitle.Text = $"Тайтл: {stream.Title}";
                txtStreamViewers.Text = $"Зрители: {stream.ViewCount}";
                txtStreamGame.Text = $"Игра: {stream.GameName}";
                txtStreamLanguage.Text = $"Язык: {stream.Language}";
            }
        }

        private async Task UpdateCompletedStreamStatsAsync(string streamerId)
        {
            var historyService = new LiveStreamHistoryService(_logRepo);
            var completedStreams = (await historyService.GetStreamsAsync(streamerId))
                .Where(s => s.EndedAt < DateTime.UtcNow)
                .ToList();

            if (!completedStreams.Any())
            {
                txtAvgViewers.Text = "-";
                txtMaxViewers.Text = "-";
                txtTotalDuration.Text = "-";
                txtStreamCount.Text = "0";
                return;
            }

            long peakViewers = completedStreams.Max(s => s.PeakViewers);
            long avgViewers = (long)completedStreams.Average(s => s.AverageViewers);
            TimeSpan avgDuration = TimeSpan.FromSeconds(completedStreams.Average(s => s.Duration.TotalSeconds));
            int count = completedStreams.Count;

            txtMaxViewers.Text = peakViewers.ToString();
            txtAvgViewers.Text = avgViewers.ToString();
            txtTotalDuration.Text = avgDuration.ToString(@"hh\:mm\:ss");
            txtStreamCount.Text = count.ToString();
        }
    }
}
