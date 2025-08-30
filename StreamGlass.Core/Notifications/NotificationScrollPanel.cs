using StreamGlass.Core.Controls;

namespace StreamGlass.Core.Notifications
{
    public class NotificationScrollPanel() : ScrollPanel<NotificationControl>()
    {
        private double m_MessageContentFontSize = 20;

        internal double MessageContentFontSize => m_MessageContentFontSize;

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (NotificationControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        public void NewNotification(Notification notification)
        {
            Dispatcher.Invoke(() =>
            {
                NotificationControl alertControl = new(this, notification, m_MessageContentFontSize);
                alertControl.NotificationMessage.Loaded += (sender, e) => { UpdateControlsPosition(); };
                AddControl(alertControl);
            });
        }

        public void ValidateLastNotification(bool validate)
        {
            Dispatcher.Invoke((Delegate)(() =>
            {
                if (Controls.Count > 0)
                    Controls[0].ValidateNotification(validate);
            }));
        }
    }
}
