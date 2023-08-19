using StreamGlass.Settings;
using StreamGlass.Controls;
using System.Windows;
using System.Windows.Controls;
using CorpseLib.Translation;
using CorpseLib.Ini;

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

        public StreamChatSettingsItem(IniSection settings, UserMessageScrollPanel streamChat, StreamGlassWindow window) : base("/Assets/chat-bubble.png", settings)
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
            DoWelcomeCheckBox.IsChecked = GetSetting("do_welcome") == "true";


            AddControlLink("sender_font_size", new NumericUpDownUserControlLink(ChatNameFont));
            AddControlLink("sender_size", new NumericUpDownUserControlLink(ChatNameWidth));
            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
            AddControlLink("panel_on_right", new CheckBoxUserControlLink(PanelOnRightCheckBox));
            AddControlLink("welcome_message", new TextBoxUserControlLink(WelcomeMessageTextBox));
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

        private void DoWelcomeCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnSave()
        {
            SetSetting("display_type", ((int)ChatModeComboBox.SelectedEnumValue).ToString());
            SetSetting("do_welcome", (DoWelcomeCheckBox.IsChecked ?? false) ? "true" : "false");
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
