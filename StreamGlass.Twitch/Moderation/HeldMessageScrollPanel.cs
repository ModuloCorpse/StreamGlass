using StreamGlass.Core;
using StreamGlass.Core.Controls;

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
            StreamGlassCanals.Register<UserMessage>("held_message", OnHeldMessage);
            StreamGlassCanals.Register<string>("held_message_moderated", RemoveHeldMessage);
        }

        public void SetSenderWidth(double width)
        {
            m_MessageSenderWidth = width;
            foreach (HeldMessage message in Controls)
                message.SetSenderNameWidth(m_MessageSenderWidth);
            UpdateControlsPosition();
        }

        public void SetSenderFontSize(double fontSize)
        {
            m_MessageSenderFontSize = fontSize;
            foreach (HeldMessage message in Controls)
                message.SetSenderNameFontSize(m_MessageSenderFontSize);
            UpdateControlsPosition();
        }

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (HeldMessage message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        private void OnHeldMessage(UserMessage? message)
        {
            if (message == null)
                return;
            Dispatcher.Invoke((Delegate)(() =>
            {
                HeldMessage chatMessage = new(this, message, m_MessageSenderWidth, m_MessageSenderFontSize, m_MessageContentFontSize);
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
