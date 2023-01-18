using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System.Windows;
using System.Windows.Controls;
using StreamFeedstock;

namespace StreamGlass.StreamChat
{
    public partial class StreamChatSettingsItem : TabItemContent
    {
        private readonly StreamGlassWindow m_Window;
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly double m_OriginalSenderWidth;
        private readonly double m_OriginalSenderFontSize;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;
        private readonly bool m_OriginalIsPanelOnRight;

        public StreamChatSettingsItem(Data settings, UserMessageScrollPanel streamChat, StreamGlassWindow window) : base("/Assets/chat-bubble.png", "chat", settings)
        {
            m_StreamChat = streamChat;
            m_Window = window;
            InitializeComponent();
            m_OriginalDisplayType = m_StreamChat.GetDisplayType();
            m_OriginalSenderWidth = m_StreamChat.MessageSenderWidth;
            m_OriginalSenderFontSize = m_StreamChat.MessageSenderFontSize;
            m_OriginalContentFontSize = m_StreamChat.MessageContentFontSize;
            m_OriginalIsPanelOnRight = m_Window.IsChatPanelOnRight;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);

            AddControlLink("sender_font_size", new NumericUpDownUserControlLink(ChatNameFont));
            AddControlLink("sender_size", new NumericUpDownUserControlLink(ChatNameWidth));
            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
            AddControlLink("panel_on_right", new CheckBoxUserControlLink(PanelOnRightCheckBox));
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
            if (ChatModeComboBox.SelectedEnumValue != m_StreamChat.GetDisplayType())
                m_StreamChat.SetDisplayType(ChatModeComboBox.SelectedEnumValue);
        }

        private void ChatNameFont_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_StreamChat.SetSenderFontSize(e.NewValue);
        }

        private void ChatNameWidth_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_StreamChat.SetSenderWidth(e.NewValue);
        }

        private void ChatMessageFont_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            m_StreamChat.SetContentFontSize(e.NewValue);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                bool isPanelOnRight = checkBox.IsChecked ?? false;
                if (isPanelOnRight != m_Window.IsChatPanelOnRight)
                {
                    if (isPanelOnRight)
                        m_Window.SetChatPanelOnRight();
                    else
                        m_Window.SetChatPanelOnLeft();
                }
            }
        }

        protected override void OnSave()
        {
            SetSetting("display_type", ((int)ChatModeComboBox.SelectedEnumValue).ToString());
        }

        protected override void OnCancel()
        {
            m_StreamChat.SetDisplayType(m_OriginalDisplayType);
            m_StreamChat.SetSenderFontSize(m_OriginalSenderFontSize);
            m_StreamChat.SetSenderWidth(m_OriginalSenderWidth);
            m_StreamChat.SetContentFontSize(m_OriginalContentFontSize);
            if (m_OriginalIsPanelOnRight)
                m_Window.SetChatPanelOnRight();
            else
                m_Window.SetChatPanelOnLeft();
        }
    }
}
