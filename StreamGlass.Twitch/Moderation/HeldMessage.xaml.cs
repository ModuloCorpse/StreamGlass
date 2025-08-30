using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Twitch.Events;
using System.Windows;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.Moderation
{
    public partial class HeldMessage : StreamGlass.Core.Controls.UserControl
    {
        private readonly HeldMessageScrollPanel m_Parent;
        private readonly Message m_HeldMessage;
        private readonly bool m_ShowBadges;

        internal string ID => m_HeldMessage.ID;

        private Text ConvertUserMessage(Message message)
        {
            Text displayedMessage = [];
            TwitchUser user = message.Sender;
            if (m_ShowBadges)
            {
                Dictionary<string, object> badgeProperties = new() {
                    { "Ratio", 0.85 },
                    { "Margin-Right", 3.0 }
                };
                foreach (TwitchBadgeInfo badgeInfo in user.Badges)
                    displayedMessage.AddImage($"url:{badgeInfo.Image[4]}", badgeProperties);
            }
            Dictionary<string, object> properties = [];
            if (!string.IsNullOrWhiteSpace(message.Color))
                properties["Color"] = message.Color;
            properties["Bold"] = true;
            displayedMessage.AddText(user.DisplayName, properties);
            displayedMessage.AddText(": ");

            foreach (Section section in message.ChatMessage)
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

        public HeldMessage(HeldMessageScrollPanel parent, Message message, double fontSize)
        {
            m_Parent = parent;
            m_HeldMessage = message;
            InitializeComponent();
            HeldMessageLabel.SetText(ConvertUserMessage(message));
            HeldMessageLabel.SetFontSize(fontSize);
        }

        public void SetMessageFontSize(double fontSize) => HeldMessageLabel.FontSize = fontSize;

        public void AllowMessage(bool allow)
        {
            StreamGlassCanals.Emit(TwitchPlugin.Canals.ALLOW_MESSAGE, new MessageAllowedEventArgs(m_HeldMessage.Sender, m_HeldMessage.ID, allow));
            m_Parent.Remove(this);
        }

        private void AllowButton_Click(object sender, RoutedEventArgs e) => AllowMessage(true);

        private void DenyButton_Click(object sender, RoutedEventArgs e) => AllowMessage(false);
    }
}
