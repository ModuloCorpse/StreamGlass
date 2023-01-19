using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System.Windows.Controls;
using StreamFeedstock;
using static StreamGlass.StreamAlert.AlertScrollPanel;
using System;

namespace StreamGlass.StreamAlert
{
    public partial class StreamAlertSettingsItem : TabItemContent
    {
        private readonly string[] m_AlertTypeNames = new string[]
        {
            "Incomming raid",
            "Outgoing raid",
            "Donation",
            "Reward",
            "Follow",
            "Tier 1",
            "Tier 2",
            "Tier 3",
            "Prime/Tier 4"
        };
        private readonly AlertInfo[] m_AlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private readonly AlertScrollPanel m_StreamAlert;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;

        public StreamAlertSettingsItem(Data settings, AlertScrollPanel streamAlert) : base("/Assets/megaphone.png", "alert", settings)
        {
            m_StreamAlert = streamAlert;
            InitializeComponent();
            m_OriginalDisplayType = m_StreamAlert.GetDisplayType();
            m_OriginalContentFontSize = m_StreamAlert.MessageContentFontSize;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);

            AlertInfosList.SetConversionDelegate(ConvertAlertType);
            foreach (AlertType type in Enum.GetValues<AlertType>())
            {
                AlertInfosList.AddObject(type);
                m_AlertInfo[(int)type] = m_StreamAlert.GetAlertInfo(type);
            }

            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
        }

        private string ConvertAlertType(object type)
        {
            if ((int)type < m_AlertTypeNames.Length)
                return m_AlertTypeNames[(int)type];
            return "ERROR";
        }

        private void AddUserType(TranslationManager translation, string key, string defaultVal)
        {
            if (translation.TryGetTranslation(key, out var ret))
                ChatModeComboBox.Items.Add(ret);
            else
                ChatModeComboBox.Items.Add(defaultVal);
        }

        private void TranslateComboBox(TranslationManager translation)
        {
            int selectedIndex = ChatModeComboBox.SelectedIndex;
            ChatModeComboBox.Items.Clear();
            AddUserType(translation, "chat_display_type_ttb", "To bottom");
            AddUserType(translation, "chat_display_type_rttb", "Reversed to bottom");
            AddUserType(translation, "chat_display_type_btt", "To top");
            AddUserType(translation, "chat_display_type_rbtt", "Reversed to top");
            ChatModeComboBox.SelectedIndex = selectedIndex;
        }

        private void TranslateAlertName(TranslationManager translation)
        {
            if (translation.TryGetTranslation("alert_name_inc_raid", out var incRaid))
                m_AlertTypeNames[(int)AlertType.INCOMMING_RAID] = incRaid;
            if (translation.TryGetTranslation("alert_name_out_raid", out var outRaid))
                m_AlertTypeNames[(int)AlertType.OUTGOING_RAID] = outRaid;
            if (translation.TryGetTranslation("alert_name_donation", out var donation))
                m_AlertTypeNames[(int)AlertType.DONATION] = donation;
            if (translation.TryGetTranslation("alert_name_reward", out var reward))
                m_AlertTypeNames[(int)AlertType.REWARD] = reward;
            if (translation.TryGetTranslation("alert_name_follow", out var follow))
                m_AlertTypeNames[(int)AlertType.FOLLOW] = follow;
            if (translation.TryGetTranslation("alert_name_tier1", out var tier1))
                m_AlertTypeNames[(int)AlertType.TIER1] = tier1;
            if (translation.TryGetTranslation("alert_name_tier2", out var tier2))
                m_AlertTypeNames[(int)AlertType.TIER2] = tier2;
            if (translation.TryGetTranslation("alert_name_tier3", out var tier3))
                m_AlertTypeNames[(int)AlertType.TIER3] = tier3;
            if (translation.TryGetTranslation("alert_name_tier4", out var tier4))
                m_AlertTypeNames[(int)AlertType.TIER4] = tier4;
        }

        protected override void OnUpdate(BrushPaletteManager palette, TranslationManager translation)
        {
            base.OnUpdate(palette, translation);
            TranslateComboBox(translation);
            TranslateAlertName(translation);
            AlertInfosList.Clear();
            foreach (AlertType type in Enum.GetValues<AlertType>())
                AlertInfosList.AddObject(type);
        }

        private void ChatModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatModeComboBox.SelectedEnumValue != m_StreamAlert.GetDisplayType())
                m_StreamAlert.SetDisplayType(ChatModeComboBox.SelectedEnumValue);
        }

        private void ChatMessageFont_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_StreamAlert.SetContentFontSize(e.NewValue);
        }

        protected override void OnSave()
        {
            SetSetting("display_type", ((int)ChatModeComboBox.SelectedEnumValue).ToString());
            foreach (AlertType type in Enum.GetValues<AlertType>())
            {
                AlertInfo alertInfo = m_AlertInfo[(int)type];
                SetSetting(string.Format("{0}_path", type), alertInfo.ImgPath);
                SetSetting(string.Format("{0}_prefix", type), alertInfo.Prefix);
                SetSetting(string.Format("{0}_enabled", type), (alertInfo.IsEnabled) ? "true" : "false");
                m_StreamAlert.SetAlertInfo(type, alertInfo);
            }
        }

        protected override void OnCancel()
        {
            m_StreamAlert.SetDisplayType(m_OriginalDisplayType);
            m_StreamAlert.SetContentFontSize(m_OriginalContentFontSize);
        }

        private void AlertInfosList_ItemEdited(object _, object e)
        {
            AlertInfo alertInfo = m_AlertInfo[(int)e];
            AlertEditor dialog = new((Window)System.Windows.Window.GetWindow(this), alertInfo);
            dialog.ShowDialog();
            AlertInfo? newInfo = dialog.AlertInfo;
            if (newInfo != null)
                m_AlertInfo[(int)e] = newInfo;
        }
    }
}
