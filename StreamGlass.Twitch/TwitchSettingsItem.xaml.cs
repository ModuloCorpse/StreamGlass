using CorpseLib.Ini;
using Microsoft.Win32;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System.Windows;

namespace StreamGlass.Twitch
{
    public partial class TwitchSettingsItem : TabItemContent
    {
        private readonly TwitchCore m_Core;
        private readonly SubModeComboBoxUserControlLink m_SubModeComboBoxUserControlLink;

        public TwitchSettingsItem(IniSection settings, TwitchCore core): base("/Assets/twitch-logo.png", settings)
        {
            InitializeComponent();
            m_Core = core;
            m_SubModeComboBoxUserControlLink = new SubModeComboBoxUserControlLink(TwitchSubModeComboBox);

            DoWelcomeCheckBox.IsChecked = GetSetting("do_welcome") == "true";

            AddControlLink("browser", new TextBoxUserControlLink(TwitchBrowserTextBox));
            AddControlLink("auto_connect", new CheckBoxUserControlLink(TwitchAutoConnectCheckBox));
            AddControlLink("public_key", new TextBoxUserControlLink(TwitchBotPublicTextBox));
            AddControlLink("secret_key", new PasswordUserControlLink(TwitchBotSecretPasswordBox, TwitchBotSecretVisibility, TwitchBotSecretVisibilityImage));
            AddControlLink("sub_mode", m_SubModeComboBoxUserControlLink);
            AddControlLink("welcome_message", new TextBoxUserControlLink(WelcomeMessageTextBox));
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
            m_Core.Connect();
        }

        protected override void OnUpdate(BrushPaletteManager palette)
        {
            base.OnUpdate(palette);
            m_SubModeComboBoxUserControlLink.TranslateComboBox();
        }

        protected override void OnSave()
        {
            SetSetting("do_welcome", (DoWelcomeCheckBox.IsChecked ?? false) ? "true" : "false");
        }
    }
}
