using Microsoft.Win32;
using StreamGlass.Core;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System.Windows;

namespace StreamGlass.Twitch
{
    public partial class SettingsItem : TabItemContent
    {
        private readonly Core m_Core;
        //private readonly SubModeComboBoxUserControlLink m_SubModeComboBoxUserControlLink;
        private readonly Settings m_Setting;

        public SettingsItem(Settings settings, Core core) : base("${ExeDir}/Assets/twitch-logo.png")
        {
            m_Setting = settings;
            InitializeComponent();
            ConnectButton.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_CONNECT);
            TwitchAutoConnectCheckBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_AUTOCONNECT);
            TwitchBrowserTextBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_BROWSER);
            TwitchBotPublicTextBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_BOT_PUBLIC);
            TwitchBotSecretPasswordBoxLbel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_BOT_PRIVATE);
            TwitchSubModeComboBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE);
            DoWelcomeLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_DO_WELCOME);
            WelcomeMessageLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_WELCOME_MESSAGE);

            m_Core = core;

            DoWelcomeCheckBox.IsChecked = m_Setting.DoWelcome;
            TwitchBrowserTextBox.Text = m_Setting.Browser;
            TwitchAutoConnectCheckBox.IsChecked = m_Setting.AutoConnect;
            TwitchBotPublicTextBox.Text = m_Setting.PublicKey;
            TwitchBotSecretPasswordBox.Text = StreamGlassVault.Load(TwitchPlugin.VaultKeys.SECRET);
            WelcomeMessageTextBox.Text = m_Setting.WelcomeMessage;

            TwitchSubModeComboBox.Items.Clear();
            TwitchSubModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_CLAIMED.ToString());
            TwitchSubModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_ALL.ToString());
            if (m_Setting.SubMode == "claimed")
                TwitchSubModeComboBox.SelectedIndex = 0;
            else
                TwitchSubModeComboBox.SelectedIndex = 1;
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
            int selectedIndex = TwitchSubModeComboBox.SelectedIndex;
            TwitchSubModeComboBox.Items.Clear();
            TwitchSubModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_CLAIMED.ToString());
            TwitchSubModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_ALL.ToString());
            TwitchSubModeComboBox.SelectedIndex = selectedIndex;
        }

        protected override void OnSave()
        {
            m_Setting.DoWelcome = DoWelcomeCheckBox.IsChecked == true;
            m_Setting.Browser = TwitchBrowserTextBox.Text;
            m_Setting.AutoConnect = TwitchAutoConnectCheckBox.IsChecked == true;
            m_Setting.PublicKey = TwitchBotPublicTextBox.Text;
            StreamGlassVault.Store(TwitchPlugin.VaultKeys.SECRET, TwitchBotSecretPasswordBox.Text);
            m_Setting.WelcomeMessage = WelcomeMessageTextBox.Text;

            if (TwitchSubModeComboBox.SelectedIndex == 0)
                m_Setting.SubMode = "claimed";
            else
                m_Setting.SubMode = "all";
        }
    }
}
