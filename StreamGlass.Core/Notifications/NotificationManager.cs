using StreamGlass.Core.Audio;

namespace StreamGlass.Core.Notifications
{
    internal class NotificationManager
    {
        private readonly NotificationScrollPanel m_NotificationScrollPanel = new();

        internal NotificationScrollPanel NotificationScrollPanel => m_NotificationScrollPanel;

        public void NewNotification(Notification notification)
        {
            m_NotificationScrollPanel.NewNotification(notification);
            if (notification.Sound != null)
                SoundManager.PlaySound(notification.Sound);
        }
    }
}
