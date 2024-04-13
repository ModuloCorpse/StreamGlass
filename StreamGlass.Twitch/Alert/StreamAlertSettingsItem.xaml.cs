using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System.Windows.Controls;

namespace StreamGlass.Twitch.Alerts
{
    public partial class StreamAlertSettingsItem : TabItemContent
    {
        private readonly AlertManager m_AlertManager;
        private readonly AlertScrollPanel m_AlertScrollPanel;
        private readonly Settings.AlertsSettings m_Settings;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;

        public StreamAlertSettingsItem(Settings.AlertsSettings settings, AlertManager alertManager, AlertScrollPanel alertScrollPanel) : base("${ExeDir}/Assets/megaphone.png")
        {
            m_Settings = settings;
            m_AlertManager = alertManager;
            m_AlertScrollPanel = alertScrollPanel;
            InitializeComponent();
            ChatMessageFontLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_CHAT_FONT);
            ChatModeComboBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_CHAT_MODE);
            AlertInfosGroupBox.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_ALERT_ALERTS);

            m_OriginalDisplayType = m_Settings.DisplayType;
            m_OriginalContentFontSize = m_Settings.MessageFontSize;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);
            ChatMessageFont.Value = m_OriginalContentFontSize;

            AlertInfosList.SetConversionDelegate((obj) => ((Alert)obj).VisualName);
            foreach (var pair in m_AlertManager.Alerts)
                AlertInfosList.AddObject(pair.Value);
        }

        private void TranslateComboBox()
        {
            int selectedIndex = ChatModeComboBox.SelectedIndex;
            ChatModeComboBox.Items.Clear();
            ChatModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.DISPLAY_TYPE_TTB.ToString());
            ChatModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.DISPLAY_TYPE_RTTB.ToString());
            ChatModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.DISPLAY_TYPE_BTT.ToString());
            ChatModeComboBox.Items.Add(TwitchPlugin.TranslationKeys.DISPLAY_TYPE_RBTT.ToString());
            ChatModeComboBox.SelectedIndex = selectedIndex;
        }

        protected override void OnUpdate(BrushPaletteManager palette)
        {
            base.OnUpdate(palette);
            TranslateComboBox();
            AlertInfosList.Clear();
            foreach (var pair in m_AlertManager.Alerts)
                AlertInfosList.AddObject(pair.Value);
        }

        private void ChatModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatModeComboBox.SelectedEnumValue != m_AlertScrollPanel.GetDisplayType())
                m_AlertScrollPanel.SetDisplayType(ChatModeComboBox.SelectedEnumValue);
        }

        private void ChatMessageFont_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_AlertScrollPanel.SetContentFontSize(e.NewValue);
        }

        protected override void OnSave()
        {
            m_Settings.DisplayType = ChatModeComboBox.SelectedEnumValue;
            m_Settings.MessageFontSize = ChatMessageFont.Value;
            foreach (var pair in m_AlertManager.Alerts)
                pair.Value.SaveSettings(m_Settings);
        }

        protected override void OnCancel()
        {
            m_AlertScrollPanel.SetDisplayType(m_OriginalDisplayType);
            m_AlertScrollPanel.SetContentFontSize(m_OriginalContentFontSize);
        }

        private void AlertInfosList_ItemEdited(object _, object e)
        {
            Alert alert = (Alert)e;
            AlertEditor dialog = new(GetWindow(), alert.Settings);
            dialog.ShowDialog();
            AlertSettings? newInfo = dialog.AlertSettings;
            if (newInfo != null)
                alert.SetSettings(newInfo);
        }
    }
}
