using System;
using System.Windows;
using TiwtchTraker.BLL;
using TwitchTracker.BLL;
using TwitchTracker.DAL;

namespace TwitchTracker
{
    public partial class MainWindow : Window
    {
        private readonly UserManager _userManager;

        public MainWindow()
        {
            InitializeComponent();

            // Создаём DAL-сервис
            var twitchService = new TwitchService("ob1bnuwy4yzi5mgjz4f4n7b24z53np", "3g9p8ap3prb8qwn4vpnrg80dg52u0f");

            // Передаём его в BLL
            _userManager = new UserManager(twitchService);
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text;

            // Запрос к BLL
            UserStats stats = await _userManager.GetUserStatsAsync(login);

            if (stats != null)
            {
                lblName.Content = $"Имя: {stats.DisplayName}";
                lblViews.Content = $"Просмотры: {stats.ViewCount}";
                lblPopularity.Content = $"Популярность: {stats.PopularityLevel}";
            }
            else
            {
                lblName.Content = "Пользователь не найден";
                lblViews.Content = "";
                lblPopularity.Content = "";
            }
        }
    }
}