using System;
using System.Windows;
using System.Windows.Controls;
using static StreamGlass.StreamChat.Chat;

namespace StreamGlass.StreamChat
{
    public partial class SettingsDialog : Window
    {
        private readonly Chat m_StreamChat;

        public SettingsDialog(Chat streamChat)
        {
            m_StreamChat = streamChat;
            InitializeComponent();
            m_StreamChat.GetBrushPalette().FillComboBox(ref ChatColorModeComboBox);
            ChatModeComboBox.Items.Add("To bottom");
            ChatModeComboBox.Items.Add("Reversed to bottom");
            ChatModeComboBox.Items.Add("To top");
            ChatModeComboBox.Items.Add("Reversed to top");
            ChatModeComboBox.SelectedIndex = 0;
            ChatNameWidth.QuietSetValue(m_StreamChat.MessageSenderWidth);
            ChatNameFont.QuietSetValue(m_StreamChat.MessageSenderFontSize);
            ChatMessageFont.QuietSetValue(m_StreamChat.MessageContentFontSize);
        }

        private void ChatColorModeComboBox_SelectionChanged(object sender, EventArgs e)
        {
            m_StreamChat.SetChatPalette(((BrushPalette.Info)ChatColorModeComboBox.SelectedItem).ID);
        }

        private void ChatModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string displayType = (string)ChatModeComboBox.SelectedItem;
            switch (displayType)
            {
                case "To bottom": m_StreamChat.SetDisplayType(DisplayType.TOP_TO_BOTTOM); break;
                case "Reversed to bottom": m_StreamChat.SetDisplayType(DisplayType.REVERSED_TOP_TO_BOTTOM); break;
                case "To top": m_StreamChat.SetDisplayType(DisplayType.BOTTOM_TO_TOP); break;
                case "Reversed to top": m_StreamChat.SetDisplayType(DisplayType.REVERSED_BOTTOM_TO_TOP); break;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
