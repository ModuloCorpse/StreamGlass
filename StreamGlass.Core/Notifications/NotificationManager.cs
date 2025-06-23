namespace StreamGlass.Core.Notifications
{
    internal class NotificationManager
    {
        private readonly NotificationScrollPanel m_NotificationScrollPanel = new();

        internal NotificationScrollPanel NotificationScrollPanel => m_NotificationScrollPanel;
    }
}
