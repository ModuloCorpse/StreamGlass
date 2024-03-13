using StreamGlass.Core;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;

namespace StreamGlass.Twitch.Moderation
{
    public class HeldMessageScrollPanel : ScrollPanel<HeldMessage>
    {
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;

        internal double MessageSenderFontSize => m_MessageSenderFontSize;
        internal double MessageSenderWidth => m_MessageSenderWidth;
        internal double MessageContentFontSize => m_MessageContentFontSize;

        public HeldMessageScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<TwitchMessage>(TwitchPlugin.Canals.HELD_MESSAGE, OnHeldMessage);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.HELD_MESSAGE_MODERATED, RemoveHeldMessage);
        }

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (HeldMessage message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        private void OnHeldMessage(TwitchMessage? message)
        {
            if (message == null)
                return;
            Dispatcher.Invoke((Delegate)(() =>
            {
                HeldMessage chatMessage = new(this, message, m_MessageContentFontSize);
                chatMessage.HeldMessageLabel.Loaded += (sender, e) => UpdateControlsPosition();
                AddControl(chatMessage);
            }));
        }

        private void RemoveHeldMessage(string? messageID)
        {
            if (messageID == null)
                return;
            Dispatcher.Invoke((Delegate)(() =>
            {
                foreach (HeldMessage message in Controls)
                {
                    if (message.ID == messageID)
                    {
                        RemoveControl(message);
                        return;
                    }
                }
            }));
        }
    }
}
