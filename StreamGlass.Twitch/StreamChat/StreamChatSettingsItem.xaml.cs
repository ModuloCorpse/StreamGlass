using CorpseLib.Ini;
using CorpseLib.Translation;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System.Windows.Controls;

namespace StreamGlass.Twitch.StreamChat
{
    public partial class StreamChatSettingsItem : TabItemContent
    {
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;

        public StreamChatSettingsItem(IniSection settings, UserMessageScrollPanel streamChat) : base("${ExeDir}/Assets/chat-bubble.png", settings)
        {
            m_StreamChat = streamChat;
            InitializeComponent();
            ChatMessageFontLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_CHAT_FONT);
            ChatModeComboBoxLabel.SetTranslationKey(TwitchPlugin.TranslationKeys.SETTINGS_CHAT_MODE);
            m_OriginalDisplayType = m_StreamChat.GetDisplayType();
            m_OriginalContentFontSize = m_StreamChat.MessageContentFontSize;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);

            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
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

        protected override void OnUpdate(BrushPaletteManager palette)
        {
            base.OnUpdate(palette);
            TranslateComboBox();
        }

        private void ChatModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatModeComboBox.SelectedEnumValue != m_StreamChat.GetDisplayType())
                m_StreamChat.SetDisplayType(ChatModeComboBox.SelectedEnumValue);
        }

        private void ChatMessageFont_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_StreamChat.SetContentFontSize(e.NewValue);
        }

        protected override void OnSave()
        {
            SetSetting("display_type", ((int)ChatModeComboBox.SelectedEnumValue).ToString());
        }

        protected override void OnCancel()
        {
            m_StreamChat.SetDisplayType(m_OriginalDisplayType);
            m_StreamChat.SetContentFontSize(m_OriginalContentFontSize);
        }
    }
}
