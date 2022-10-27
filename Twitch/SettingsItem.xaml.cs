using Microsoft.Win32;
using StreamGlass.Settings;
using System;
using System.Windows;

namespace StreamGlass.Twitch
{
    public partial class SettingsItem : TabItem
    {
        private readonly Bot m_Bot;

        public SettingsItem(Data settings, Bot bot): base("/Assets/twitch-logo.png", "twitch", settings)
        {
            InitializeComponent();
            m_Bot = bot;

            AddControlLink("browser", new TextBoxUserControlLink(TwitchBrowserTextBox));
            AddControlLink("auto_connect", new CheckBoxUserControlLink(TwitchAutoConnectCheckBox));
            AddControlLink("channel", new TextBoxUserControlLink(TwitchChannelTextBox));
            AddControlLink("public_key", new TextBoxUserControlLink(TwitchBotPublicTextBox));
            AddControlLink("secret_key", new PasswordUserControlLink(TwitchBotSecretPasswordBox, TwitchBotSecretTextBox, TwitchBotSecretVisibility, TwitchBotSecretVisibilityImage));
        }

        private void TwitchBrowserFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = "C:\\",
                Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
                TwitchBrowserTextBox.Text = openFileDialog.FileName;
        }

        private void TwitchConnectButton_Click(object sender, RoutedEventArgs e)
        {
            m_Bot.Connect();
        }
    }
}
