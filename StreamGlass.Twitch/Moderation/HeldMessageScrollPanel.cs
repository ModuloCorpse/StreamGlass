using StreamGlass.Core;
using StreamGlass.Core.Controls;

namespace StreamGlass.Twitch.Moderation
{
    public class HeldMessageScrollPanel : ScrollPanel<HeldMessage>
    {
        private double m_MessageContentFontSize = 14;

        public HeldMessageScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<TwitchMessage>(TwitchPlugin.Canals.HELD_MESSAGE, OnHeldMessage);
            StreamGlassCanals.Register<string>(TwitchPlugin.Canals.HELD_MESSAGE_MODERATED, RemoveHeldMessage);
            StreamGlassCanals.Register<bool>(TwitchPlugin.Canals.ALLOW_AUTOMOD, AllowLastMessage);
        }

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (HeldMessage message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        private void AllowLastMessage(bool allow)
        {
            Dispatcher.Invoke((Delegate)(() =>
            {
                if (Controls.Count > 0)
                    Controls[0].AllowMessage(allow);
            }));
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
