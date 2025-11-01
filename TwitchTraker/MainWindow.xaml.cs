using System;
using System.Windows.Media.Imaging;

using System.Windows;
using System.Windows.Media;
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

            TwitchService twitchService = new TwitchService("ob1bnuwy4yzi5mgjz4f4n7b24z53np", "3vzsiqkzs2df4dnftencog3cpva46z",null);
            _userManager = new UserManager(twitchService);
        }


        public void drawProfileImage(string profileImageUrl)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(profileImageUrl);
            bitmap.EndInit();
            ProfileImageUI.Source = bitmap;
            
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            String login = SearchTextBox.Text;
            Console.WriteLine("Search text: " + login);

            MainPanel.VerticalAlignment = VerticalAlignment.Top;
            StatsPanel.Visibility = Visibility.Visible;
           
            
            UserStats stats =  await _userManager.GetUserStatsAsync(login);
            
            if (stats != null)
            {
                if (stats.isLive == true)
                {
                    isLiveUI.Foreground = Brushes.Red;
                    isLiveUI.Text = "В эфире";
                }
                else
                {
                    isLiveUI.Foreground = Brushes.LightGray;
                    isLiveUI.Text = "Оффлайн";
                }
                DislplayNameUI.Text = "Отоброжаемое " + stats.displayName;
                drawProfileImage(stats.profileImageUrl);
            }
            else
            {
                DislplayNameUI.Text = "Пользователь не найден ";
            }
            
            
            
           
            
            
        }
    }

}