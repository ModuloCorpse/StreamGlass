using CorpseLib.Translation;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Settings;
using System.Windows.Controls;

namespace StreamGlass.Core.StreamChat
{
    public partial class StreamChatSettingsItem : TabItemContent
    {
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly BaseSettings.ChatSettings m_Settings = new();
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;

        public StreamChatSettingsItem(BaseSettings.ChatSettings settings, UserMessageScrollPanel streamChat) : base("${ExeDir}/Assets/chat-bubble.png")
        {
            m_StreamChat = streamChat;
            m_Settings = settings;
            m_Settings.DisplayType = m_StreamChat.GetDisplayType();
            m_Settings.MessageFontSize = m_StreamChat.MessageContentFontSize;

            InitializeComponent();
            ChatMessageFontLabel.SetTranslationKey(StreamGlassTranslationKeys.SETTINGS_CHAT_FONT);
            ChatModeComboBoxLabel.SetTranslationKey(StreamGlassTranslationKeys.SETTINGS_CHAT_MODE);

            m_OriginalDisplayType = m_Settings.DisplayType;
            m_OriginalContentFontSize = m_Settings.MessageFontSize;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);
            ChatMessageFont.Value = m_OriginalContentFontSize;
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
            m_Settings.DisplayType = ChatModeComboBox.SelectedEnumValue;
            m_Settings.MessageFontSize = ChatMessageFont.Value;
        }

        protected override void OnCancel()
        {
            m_StreamChat.SetDisplayType(m_OriginalDisplayType);
            m_StreamChat.SetContentFontSize(m_OriginalContentFontSize);
        }
    }
}
