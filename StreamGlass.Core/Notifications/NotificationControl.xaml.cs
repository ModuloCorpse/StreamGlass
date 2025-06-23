using CorpseLib.Wpf;
using StreamGlass.Core.Controls;
using System.Windows;

namespace StreamGlass.Core.Notifications
{
    public partial class NotificationControl : UserControl
    {
        private readonly NotificationScrollPanel m_Parent;
        private readonly Notification m_Notification;

        public NotificationControl(NotificationScrollPanel parent, Notification notification, double contentFontSize)
        {
            m_Parent = parent;
            m_Notification = notification;
            InitializeComponent();
            NotificationImage.Source = ImageLoader.LoadStaticImage(notification.ImagePath)?.Source;
            NotificationMessage.SetText(notification.Message);
            NotificationMessage.SetFontSize(contentFontSize);
            MessagePanel.BrushPaletteKey = "chat_background";
        }

        public double MessageFontSize { get => NotificationMessage.FontSize; }

        public void SetMessageFontSize(double fontSize) => NotificationMessage.SetFontSize(fontSize);

        public void ValidateNotification(bool validate)
        {
            m_Notification.ValidateNotification(validate);
            m_Parent.Remove(this);
        }

        private void AllowButton_Click(object sender, RoutedEventArgs e) => ValidateNotification(true);

        private void DenyButton_Click(object sender, RoutedEventArgs e) => ValidateNotification(false);
    }
}
