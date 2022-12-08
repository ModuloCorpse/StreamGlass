using Microsoft.Win32;
using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System.Windows;
using StreamFeedstock;

namespace StreamGlass.Twitch
{
    public partial class TwitchSettingsItem : TabItem
    {
        private readonly Bot m_Bot;
        private readonly SubModeComboBoxUserControlLink m_SubModeComboBoxUserControlLink;

        public TwitchSettingsItem(Data settings, Bot bot): base("/Assets/twitch-logo.png", "twitch", settings)
        {
            InitializeComponent();
            m_Bot = bot;
            m_SubModeComboBoxUserControlLink = new SubModeComboBoxUserControlLink(TwitchSubModeComboBox);

            AddControlLink("browser", new TextBoxUserControlLink(TwitchBrowserTextBox));
            AddControlLink("auto_connect", new CheckBoxUserControlLink(TwitchAutoConnectCheckBox));
            AddControlLink("channel", new TextBoxUserControlLink(TwitchChannelTextBox));
            AddControlLink("public_key", new TextBoxUserControlLink(TwitchBotPublicTextBox));
            AddControlLink("secret_key", new PasswordUserControlLink(TwitchBotSecretPasswordBox, TwitchBotSecretVisibility, TwitchBotSecretVisibilityImage));
            AddControlLink("sub_mode", m_SubModeComboBoxUserControlLink);
        }

        private void TwitchBrowserFileDialog_Click(object sender, RoutedEventArgs e)
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

        protected override void OnUpdate(BrushPaletteManager palette, TranslationManager translation)
        {
            base.OnUpdate(palette, translation);
            m_SubModeComboBoxUserControlLink.TranslateComboBox(translation);
        }
    }
}
