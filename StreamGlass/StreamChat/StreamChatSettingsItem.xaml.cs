using StreamGlass.Core.Settings;
using StreamGlass.Core.Controls;
using System.Windows;
using System.Windows.Controls;
using CorpseLib.Translation;
using CorpseLib.Ini;

namespace StreamGlass.StreamChat
{
    public partial class StreamChatSettingsItem : TabItemContent
    {
        private readonly MainWindow m_Window;
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly double m_OriginalContentFontSize;
        private readonly ScrollPanelDisplayType m_OriginalDisplayType;
        private readonly bool m_OriginalIsPanelOnRight;

        public StreamChatSettingsItem(IniSection settings, UserMessageScrollPanel streamChat, MainWindow window) : base("/Assets/chat-bubble.png", settings)
        {
            m_StreamChat = streamChat;
            m_Window = window;
            InitializeComponent();
            m_OriginalDisplayType = m_StreamChat.GetDisplayType();
            m_OriginalContentFontSize = m_StreamChat.MessageContentFontSize;
            m_OriginalIsPanelOnRight = m_Window.IsChatPanelOnRight;

            ChatModeComboBox.SetSelectedEnumValue(m_OriginalDisplayType);

            AddControlLink("message_font_size", new NumericUpDownUserControlLink(ChatMessageFont));
            AddControlLink("panel_on_right", new CheckBoxUserControlLink(PanelOnRightCheckBox));
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
            m_StreamChat.SetContentFontSize(m_OriginalContentFontSize);
            if (m_OriginalIsPanelOnRight)
                m_Window.SetChatPanelOnRight();
            else
                m_Window.SetChatPanelOnLeft();
        }
    }
}
