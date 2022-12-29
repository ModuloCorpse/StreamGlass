using StreamGlass.Settings;
using StreamFeedstock.Controls;
using System.Windows;
using System.Windows.Controls;
using TabItem = StreamGlass.Settings.TabItem;
using StreamFeedstock;

namespace StreamGlass.StreamChat
{
    public partial class StreamChatSettingsItem : TabItem
    {
        private readonly StreamGlassWindow m_Window;
        private readonly UserMessageScrollPanel m_StreamChat;

        public StreamChatSettingsItem(Data settings, UserMessageScrollPanel streamChat, StreamGlassWindow window) : base("/Assets/chat-bubble.png", "stream-chat", settings)
        {
            m_StreamChat = streamChat;
            m_Window = window;
            InitializeComponent();
            ChatModeComboBox.Items.Add("To bottom");
            ChatModeComboBox.Items.Add("Reversed to bottom");
            ChatModeComboBox.Items.Add("To top");
            ChatModeComboBox.Items.Add("Reversed to top");
            ChatModeComboBox.SelectedIndex = 0;
            ChatNameWidth.QuietSetValue(m_StreamChat.MessageSenderWidth);
            ChatNameFont.QuietSetValue(m_StreamChat.MessageSenderFontSize);
            ChatMessageFont.QuietSetValue(m_StreamChat.MessageContentFontSize);
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
            string displayType = (string)ChatModeComboBox.SelectedItem;
            switch (displayType)
            {
                case "To bottom": m_StreamChat.SetDisplayType(ScrollPanelDisplayType.TOP_TO_BOTTOM); break;
                case "Reversed to bottom": m_StreamChat.SetDisplayType(ScrollPanelDisplayType.REVERSED_TOP_TO_BOTTOM); break;
                case "To top": m_StreamChat.SetDisplayType(ScrollPanelDisplayType.BOTTOM_TO_TOP); break;
                case "Reversed to top": m_StreamChat.SetDisplayType(ScrollPanelDisplayType.REVERSED_BOTTOM_TO_TOP); break;
            }
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
                if (checkBox.IsChecked != null && checkBox.IsChecked.Value)
                    m_Window.SetChatPanelOnRight();
                else
                    m_Window.SetChatPanelOnLeft();
            }
        }
    }
}
