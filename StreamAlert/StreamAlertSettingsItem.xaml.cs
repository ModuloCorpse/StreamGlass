using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System.Windows.Controls;
using StreamFeedstock;

namespace StreamGlass.StreamAlert
{
    public partial class StreamAlertSettingsItem : TabItemContent
    {
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

            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
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

        protected override void OnUpdate(BrushPaletteManager palette, TranslationManager translation)
        {
            base.OnUpdate(palette, translation);
            TranslateComboBox(translation);
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
        }

        protected override void OnCancel()
        {
            m_StreamAlert.SetDisplayType(m_OriginalDisplayType);
            m_StreamAlert.SetContentFontSize(m_OriginalContentFontSize);
        }
    }
}
