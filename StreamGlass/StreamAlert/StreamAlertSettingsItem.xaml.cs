using StreamGlass.Core.Settings;
using StreamGlass.Core.Controls;
using System.Windows.Controls;
using static StreamGlass.StreamAlert.AlertScrollPanel;
using System;
using CorpseLib.Translation;
using CorpseLib.Ini;

namespace StreamGlass.StreamAlert
{
    public partial class StreamAlertSettingsItem : TabItemContent
    {
        private readonly string[] m_AlertTypeNames = [
            "Incomming raid",
            "Outgoing raid",
            "Donation",
            "Reward",
            "Follow",
            "Tier 1",
            "Tier 2",
            "Tier 3",
            "Prime/Tier 4",
            "Shoutout",
            "Being shoutout"
        ];
        private readonly AlertInfo[] m_GiftAlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private readonly AlertInfo[] m_AlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private readonly AlertScrollPanel m_StreamAlert;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;

        public StreamAlertSettingsItem(IniSection settings, AlertScrollPanel streamAlert) : base("/Assets/megaphone.png", settings)
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

            GiftAlertInfosList.SetConversionDelegate(ConvertAlertType);
            foreach (AlertType type in Enum.GetValues<AlertType>())
            {
                GiftAlertInfosList.AddObject(type);
                m_GiftAlertInfo[(int)type] = m_StreamAlert.GetGiftAlertInfo(type);
            }

            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
        }

        private string ConvertAlertType(object type)
        {
            if ((int)type < m_AlertTypeNames.Length)
                return m_AlertTypeNames[(int)type];
            return "ERROR";
        }

        private void TranslateComboBox()
        {
            int selectedIndex = ChatModeComboBox.SelectedIndex;
            ChatModeComboBox.Items.Clear();
            ChatModeComboBox.Items.Add(Translator.Translate("${chat_display_type_ttb}"));
            ChatModeComboBox.Items.Add(Translator.Translate("${chat_display_type_rttb}"));
            ChatModeComboBox.Items.Add(Translator.Translate("${chat_display_type_btt}"));
            ChatModeComboBox.Items.Add(Translator.Translate("${chat_display_type_rbtt}"));
            ChatModeComboBox.SelectedIndex = selectedIndex;
        }

        private void TranslateAlertName()
        {
            m_AlertTypeNames[(int)AlertType.INCOMMING_RAID] = Translator.Translate("${alert_name_inc_raid}");
            m_AlertTypeNames[(int)AlertType.OUTGOING_RAID] = Translator.Translate("${alert_name_out_raid}");
            m_AlertTypeNames[(int)AlertType.DONATION] = Translator.Translate("${alert_name_donation}");
            m_AlertTypeNames[(int)AlertType.REWARD] = Translator.Translate("${alert_name_reward}");
            m_AlertTypeNames[(int)AlertType.FOLLOW] = Translator.Translate("${alert_name_follow}");
            m_AlertTypeNames[(int)AlertType.TIER1] = Translator.Translate("${alert_name_tier1}");
            m_AlertTypeNames[(int)AlertType.TIER2] = Translator.Translate("${alert_name_tier2}");
            m_AlertTypeNames[(int)AlertType.TIER3] = Translator.Translate("${alert_name_tier3}");
            m_AlertTypeNames[(int)AlertType.TIER4] = Translator.Translate("${alert_name_tier4}");
        }

        protected override void OnUpdate(BrushPaletteManager palette)
        {
            base.OnUpdate(palette);
            TranslateComboBox();
            TranslateAlertName();
            AlertInfosList.Clear();
            foreach (AlertType type in Enum.GetValues<AlertType>())
                AlertInfosList.AddObject(type);
            GiftAlertInfosList.Clear();
            foreach (AlertType type in Enum.GetValues<AlertType>())
                GiftAlertInfosList.AddObject(type);
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
                AlertInfo? giftAlertInfo = m_GiftAlertInfo[(int)type];
                if (giftAlertInfo != null)
                {
                    SetSetting(string.Format("{0}_gift_path", type), giftAlertInfo.ImgPath);
                    SetSetting(string.Format("{0}_gift_prefix", type), giftAlertInfo.Prefix);
                    SetSetting(string.Format("{0}_gift_enabled", type), (giftAlertInfo.IsEnabled) ? "true" : "false");
                    m_StreamAlert.SetGiftAlertInfo(type, giftAlertInfo);
                }
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
            AlertEditor dialog = new(GetWindow(), alertInfo);
            dialog.ShowDialog();
            AlertInfo? newInfo = dialog.AlertInfo;
            if (newInfo != null)
                m_AlertInfo[(int)e] = newInfo;
        }

        private void GiftAlertInfosList_ItemEdited(object _, object e)
        {
            AlertInfo alertInfo = m_GiftAlertInfo[(int)e];
            AlertEditor dialog = new(GetWindow(), alertInfo);
            dialog.ShowDialog();
            AlertInfo? newInfo = dialog.AlertInfo;
            if (newInfo != null)
                m_GiftAlertInfo[(int)e] = newInfo;
        }
    }
}
