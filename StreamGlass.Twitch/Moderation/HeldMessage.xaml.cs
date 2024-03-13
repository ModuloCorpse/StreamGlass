using StreamGlass.Core;
using StreamGlass.Core.Profile;
using StreamGlass.Twitch.Events;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TwitchCorpse.API;
using TwitchCorpse;
using CorpseLib.StructuredText;

namespace StreamGlass.Twitch.Moderation
{
    public partial class HeldMessage : Core.Controls.UserControl
    {
        private readonly HeldMessageScrollPanel m_Parent;
        private readonly TwitchMessage m_HeldMessage;
        private readonly bool m_ShowBadges;

        internal string ID => m_HeldMessage.ID;

        private Text ConvertUserMessage(TwitchMessage message)
        {
            Text displayedMessage = new();
            TwitchUser user = message.Sender;
            if (m_ShowBadges)
            {
                Dictionary<string, object> badgeProperties = new() {
                    { "Ratio", 0.85 },
                    { "Margin-Right", 3.0 }
                };
                foreach (TwitchBadgeInfo badgeInfo in user.Badges)
                    displayedMessage.AddImage(badgeInfo.URL4x, badgeProperties);
            }
            Dictionary<string, object> properties = [];
            if (!string.IsNullOrWhiteSpace(message.Color))
                properties["Color"] = message.Color;
            properties["Bold"] = true;
            displayedMessage.AddText(user.DisplayName, properties);
            displayedMessage.AddText(": ");

            Dictionary<string, object> emoteProperties = new() { { "Ratio", 1.5 } };
            foreach (Section section in message.Message)
            {
                switch (section.SectionType)
                {
                    case Section.Type.TEXT:
                    {
                        displayedMessage.AddText(section.Content);
                        break;
                    }
                    case Section.Type.IMAGE:
                    {
                        displayedMessage.AddImage(section.Content, section.Alt, emoteProperties);
                        break;
                    }
                    case Section.Type.ANIMATED_IMAGE:
                    {
                        displayedMessage.AddAnimatedImage(section.Content, section.Alt, emoteProperties);
                        break;
                    }
                }
            }
            return displayedMessage;
        }

        public HeldMessage(HeldMessageScrollPanel parent, TwitchMessage message, double fontSize)
        {
            m_Parent = parent;
            m_HeldMessage = message;
            InitializeComponent();
            HeldMessageLabel.SetText(ConvertUserMessage(message));
            HeldMessageLabel.SetFontSize(fontSize);
        }

        public void SetMessageFontSize(double fontSize) => HeldMessageLabel.FontSize = fontSize;

        private void AllowButton_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassCanals.Emit(TwitchPlugin.Canals.ALLOW_MESSAGE, new MessageAllowedEventArgs(m_HeldMessage.Sender, m_HeldMessage.ID, true));
            m_Parent.Remove(this);
        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassCanals.Emit(TwitchPlugin.Canals.ALLOW_MESSAGE, new MessageAllowedEventArgs(m_HeldMessage.Sender, m_HeldMessage.ID, false));
            m_Parent.Remove(this);
        }
    }
}
