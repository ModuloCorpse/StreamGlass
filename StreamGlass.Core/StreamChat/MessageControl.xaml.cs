using CorpseLib.StructuredText;
using StreamGlass.Core.Controls;
using System.Windows;
using System.Windows.Media;

namespace StreamGlass.Core.StreamChat
{
    public partial class MessageControl : UserControl
    {
        private readonly UserMessageScrollPanel m_StreamChat;
        private readonly Message m_Message;
        private readonly Message? m_ReplyMessage;
        private readonly bool m_IsHighlighted;
        private readonly bool m_ShowBadges;

        private static Text ConvertUserReply(Message message)
        {
            Text displayedMessage = [];
            ChatUser user = message.User;
            displayedMessage.AddText("⤷");
            displayedMessage.AddText(user.Name);
            displayedMessage.AddText(": ");

            foreach (Section section in message.Content)
            {
                switch (section.SectionType)
                {
                    case Section.Type.TEXT:
                    {
                        displayedMessage.AddText(section.Content, section.Properties);
                        break;
                    }
                    case Section.Type.IMAGE:
                    {
                        Dictionary<string, object> imageProperties = section.Properties;
                        imageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddImage(section.Content, section.Alt, imageProperties);
                        break;
                    }
                    case Section.Type.ANIMATED_IMAGE:
                    {
                        Dictionary<string, object> animatedImageProperties = section.Properties;
                        animatedImageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddAnimatedImage(section.Content, section.Alt, animatedImageProperties);
                        break;
                    }
                }
            }
            return displayedMessage;
        }

        private Text ConvertUserMessage(Message message)
        {
            Text displayedMessage = [];
            ChatUser user = message.User;
            /*if (m_ShowSourceLogo)
            {

            }*/

            if (m_ShowBadges)
            {
                Dictionary<string, object> badgeProperties = new() {
                    { "Ratio", 0.85 },
                    { "Margin-Right", 3.0 }
                };
                foreach (string badge in message.Badges)
                    displayedMessage.AddImage(badge, badgeProperties);
            }

            Dictionary<string, object> properties = [];
            if (user.Color != null)
                properties["Color"] = (Color)user.Color;
            properties["Bold"] = true;
            displayedMessage.AddText(user.Name, properties);
            displayedMessage.AddText(": ");

            foreach (Section section in message.Content)
            {
                switch (section.SectionType)
                {
                    case Section.Type.TEXT:
                    {
                        displayedMessage.AddText(section.Content, section.Properties);
                        break;
                    }
                    case Section.Type.IMAGE:
                    {
                        Dictionary<string, object> imageProperties = section.Properties;
                        imageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddImage(section.Content, section.Alt, imageProperties);
                        break;
                    }
                    case Section.Type.ANIMATED_IMAGE:
                    {
                        Dictionary<string, object> animatedImageProperties = section.Properties;
                        animatedImageProperties.TryAdd("Ratio", 1.5);
                        displayedMessage.AddAnimatedImage(section.Content, section.Alt, animatedImageProperties);
                        break;
                    }
                }
            }
            return displayedMessage;
        }

        public MessageControl(UserMessageScrollPanel streamChatPanel, Message message, Message? reply, double contentFontSize, bool isHighligted, bool showBadges)
        {
            m_StreamChat = streamChatPanel;
            m_Message = message;
            m_ReplyMessage = reply;
            m_ShowBadges = showBadges;
            InitializeComponent();
            UpdateContextMenu();
            MessageContent.SetText(ConvertUserMessage(message));
            MessageContent.SetFontSize(contentFontSize);

            if (m_ReplyMessage != null)
            {
                ReplyContent.Visibility = Visibility.Visible;
                ReplyContent.SetText(ConvertUserReply(m_ReplyMessage));
                ReplyContent.SetFontSize(contentFontSize * 0.8);
            }
            else
                ReplyContent.Visibility = Visibility.Collapsed;

            m_IsHighlighted = (isHighligted || message.IsHighlighted);
            if (m_IsHighlighted)
            {
                MessagePanel.BrushPaletteKey = "chat_highlight_background";
                MessageContent.BrushPaletteKey = "chat_highlight_background";
                MessageContent.TextBrushPaletteKey = "chat_highlight_message";
            }
            else
                MessagePanel.BrushPaletteKey = "chat_background";

            if (message.BorderColor != null)
                AnnouncementBorder.BorderBrush = new SolidColorBrush((Color)message.BorderColor);
            else
                AnnouncementBorder.BorderBrush = Brushes.Transparent;
        }

        public Message TwitchMessage => m_Message;
        public string UserID => m_Message.User.ID;
        public string ID => m_Message.ID;

        public void SetMessageFontSize(double fontSize) => MessageContent.SetFontSize(fontSize);

        public void UpdateContextMenu()
        {
            //TODO Handle source specific context menu
            MessageContextMenu.Items.Clear();
            foreach (var item in m_StreamChat.ContextMenuActions)
            {
                MenuItem menuItem = new();
                menuItem.SetTranslationKey(item.Key);
                menuItem.Click += (sender, e) => { item.Value.Invoke(GetWindow()!, m_Message); };
                MessageContextMenu.Items.Add(menuItem);
            }
        }

        public void Refresh()
        {
            MessageContent.SetText(ConvertUserMessage(m_Message));
            if (m_ReplyMessage != null)
                ReplyContent.SetText(ConvertUserReply(m_ReplyMessage));
        }
    }
}
