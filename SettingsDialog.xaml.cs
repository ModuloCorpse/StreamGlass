using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StreamGlass
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        private bool m_ShowTwitchBotSecret = false;
        private readonly Settings m_Settings;
        private readonly TwitchBot m_Bot;

        public SettingsDialog(Settings settings, TwitchBot bot)
        {
            InitializeComponent();
            m_Settings = settings;
            m_Bot = bot;

            SystemBrowserTextBox.Text = m_Settings.Get("system", "browser");
            //Twitch
            AutoConnectCheckBox.IsChecked = m_Settings.Get("twitch", "auto_connect") == "true";
            TwitchChannelTextBox.Text = m_Settings.Get("twitch", "channel");
            TwitchBotPublicTextBox.Text = m_Settings.Get("twitch", "public_key");
            TwitchBotSecretTextBox.Text = m_Settings.Get("twitch", "secret_key");
            TwitchBotSecretPasswordBox.Password = m_Settings.Get("twitch", "secret_key");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_Settings.Set("system", "browser", SystemBrowserTextBox.Text);
            //Twitch
            m_Settings.Set("twitch", "auto_connect", (AutoConnectCheckBox.IsChecked != null && (bool)AutoConnectCheckBox.IsChecked) ? "true" : "false");
            m_Settings.Set("twitch", "channel", TwitchChannelTextBox.Text);
            m_Settings.Set("twitch", "public_key", TwitchBotPublicTextBox.Text);
            if (m_ShowTwitchBotSecret)
                m_Settings.Set("twitch", "secret_key", TwitchBotSecretTextBox.Text);
            else
                m_Settings.Set("twitch", "secret_key", TwitchBotSecretPasswordBox.Password);
            Close();
        }

        private void SystemBrowserFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = "C:\\",
                Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
                SystemBrowserTextBox.Text = openFileDialog.FileName;
        }

        private void TwitchBotSecretVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (!m_ShowTwitchBotSecret)
            {
                TwitchBotSecretTextBox.Text = TwitchBotSecretPasswordBox.Password;
                TwitchBotSecretPasswordBox.Visibility = Visibility.Collapsed;
                TwitchBotSecretTextBox.Visibility = Visibility.Visible;
                TwitchBotSecretVisibilityImage.Source = new BitmapImage(new Uri(@"/Assets/sight-enabled.png", UriKind.Relative));
            }
            else
            {
                TwitchBotSecretPasswordBox.Password = TwitchBotSecretTextBox.Text;
                TwitchBotSecretPasswordBox.Visibility = Visibility.Visible;
                TwitchBotSecretTextBox.Visibility = Visibility.Collapsed;
                TwitchBotSecretVisibilityImage.Source = new BitmapImage(new Uri(@"/Assets/sight-disabled.png", UriKind.Relative));
            }
            m_ShowTwitchBotSecret = !m_ShowTwitchBotSecret;
        }

        private void TwitchConnectButton_Click(object sender, RoutedEventArgs e)
        {
            m_Bot.Connect();
        }
    }
}
