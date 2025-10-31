using System;
using System.Windows;
using TiwtchTraker.BLL;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
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

            TwitchService twitchService = new TwitchService("client_id", "client_secret","Acssec token");
            _userManager = new UserManager(twitchService);
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            String login = SearchTextBox.Text;
            Console.WriteLine("Search text: " + login);
            
            MainPanel.Visibility = Visibility.Collapsed;
            StatsPanel.Visibility = Visibility.Visible;
            UserStats stats =  await _userManager.GetUserStatsAsync(login);
            
            if (stats != null)
            {
                UserNameUI.Text = "Имя пользователя: "+stats.username;
            }
            else
            {
                UserNameUI.Text = "Пользователь не найден ";
            }
            
            
            
           
            
            
        }
    }

}