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
using TwitchTracker.Services;

namespace TwitchTrackerUI
{
    public partial class MainWindow : Window
    {
        private readonly StreamerStats _stats;
        private readonly TrackedStreamersService _tracked;
        private readonly ILiveStreamLogRepository _logRepo;
        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            var host = Host.CreateDefaultBuilder() //Создаём хост приложения с DI
                .ConfigureServices((context, services) =>// регимтрация сепвисов
                {
                    services.AddSingleton<ITwitchServices>(_ => new TwitchServices(
                        "ob1bnuwy4yzi5mgjz4f4n7b24z53np",
                        "jmxl64wg5aeuezx5lmf7ofdfoufta0"));

                    services.AddSingleton<ILiveStreamLogRepository, JsonLiveStreamLogRepository>();
                    services.AddSingleton<TrackedStreamersService>();
                    services.AddSingleton<StreamerStats>();
                    services.AddHostedService<LiveStreamLoggingService>();
                })
                .Build();

            host.Start(); // запуск хоста
            //Получаем зарегистрированные сервисы
            _stats = host.Services.GetRequiredService<StreamerStats>();
            _tracked = host.Services.GetRequiredService<TrackedStreamersService>();
            _logRepo = host.Services.GetRequiredService<ILiveStreamLogRepository>();

            // Инициализация графиков репозиторием логов
            allHourlyViewersChart.Initialize(_logRepo);
            allHourlyViewersChart.Start();

            gamePieChart.Initialize(_logRepo);
            hourlyViewersChart.Initialize(_logRepo);

            //обработчики событий
            btnSearch.Click += BtnSearch_Click;
            btnHome.Click += BtnHome_Click;

            // Таймер для обновления текущего стрима и метрик
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _timer.Tick += async (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtStreamerLogin.Text))
                    await RefreshStreamerDataAsync(txtStreamerLogin.Text.Trim());
            };
            _timer.Start();
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e) //Обработчик кнопки поиска:
        {
            string login = txtStreamerLogin.Text.Trim();
            if (string.IsNullOrEmpty(login)) return;

            _tracked.Add(login);
            await ShowStreamerViewAsync(login);
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e) //Обработчик кнопки "Главная":
        {
            spStreamerInfo.Visibility = Visibility.Collapsed;
            allHourlyViewersChart.Visibility = Visibility.Visible;
        }

        private async Task ShowStreamerViewAsync(string login) //  Метод отображения данных стримера
        {
            var streamer = await _stats.GetStreamerInfoAsync(login); //Получает информацию о стримере
            if (streamer == null)
            {
                MessageBox.Show("Стример не найден.");
                return;
            }

            allHourlyViewersChart.Visibility = Visibility.Collapsed;
            spStreamerInfo.Visibility = Visibility.Visible;

            imgAvatar.Source = new BitmapImage(new Uri(streamer.Avatar));
            txtDisplayName.Text = streamer.DisplayName;
            txtFollowers.Text = $"Подписчики: {streamer.TotalFollowers}";
            txtBio.Text = streamer.Bio;

            var stream = await _stats.GetLiveStreamAsync(login);
            if (stream.IsLive) //Если стример онлайн
            {
                brdLiveStream.Background = new SolidColorBrush(Color.FromRgb(145, 70, 255));
                ellipseLive.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0));
                txtStreamStatus.Text = "В эфире";
                txtStreamTitle.Text = $"Тайтл: {stream.Title}";
                txtStreamViewers.Text = $"Зрители: {stream.ViewCount}";
                txtStreamGame.Text = $"Игра: {stream.GameName}";
                txtStreamLanguage.Text = $"Язык: {stream.Language}";
            }
            else
            {
                brdLiveStream.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                ellipseLive.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                txtStreamStatus.Text = "Оффлайн";
                txtStreamTitle.Text = "";
                txtStreamViewers.Text = "";
                txtStreamGame.Text = "";
                txtStreamLanguage.Text = "";
            }

            await UpdateCompletedStreamStatsAsync(streamer.StreamerId); //Обновляем статистику по завершённым стримам

            gamePieChart.SetStreamer(streamer.StreamerId);
            hourlyViewersChart.SetStreamer(streamer.StreamerId);
        }

        private async Task RefreshStreamerDataAsync(string login)//Метод для таймера
        {
            await ShowStreamerViewAsync(login);
        }

        private async Task UpdateCompletedStreamStatsAsync(string streamerId) //Получаем историю завершённых стримов стримера из логов
        {
            var historyService = new LiveStreamHistoryService(_logRepo);
            var completedStreams = (await historyService.GetStreamsAsync(streamerId))
                .Where(s => s.EndedAt < DateTime.UtcNow)
                .ToList(); //берёт только те стримы, которые уже завершены

            if (!completedStreams.Any()) //Если стримов нет
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
