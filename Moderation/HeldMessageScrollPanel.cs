using StreamGlass.Controls;
using StreamGlass.StreamChat;
using System;

namespace StreamGlass.Moderation
{
    public class HeldMessageScrollPanel : ScrollPanel<HeldMessage>
    {
        private double m_MessageSenderFontSize = 14;
        private double m_MessageSenderWidth = 100;
        private double m_MessageContentFontSize = 14;

        public HeldMessageScrollPanel() : base()
        {
            StreamGlassCanals.HELD_MESSAGE.Register(OnHeldMessage);
            StreamGlassCanals.HELD_MESSAGE_MODERATED.Register(RemoveHeldMessage);
        }

        internal double MessageSenderFontSize => m_MessageSenderFontSize;
        internal double MessageSenderWidth => m_MessageSenderWidth;
        internal double MessageContentFontSize => m_MessageContentFontSize;

        internal void SetSenderWidth(double width)
        {
            m_MessageSenderWidth = width;
            foreach (HeldMessage message in Controls)
                message.SetSenderNameWidth(m_MessageSenderWidth);
            UpdateControlsPosition();
        }

        internal void SetSenderFontSize(double fontSize)
        {
            m_MessageSenderFontSize = fontSize;
            foreach (HeldMessage message in Controls)
                message.SetSenderNameFontSize(m_MessageSenderFontSize);
            UpdateControlsPosition();
        }

        internal void SetContentFontSize(double fontSize)
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
