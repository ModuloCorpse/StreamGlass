using StreamGlass.Controls;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StreamGlass.Settings
{
    public class PasswordUserControlLink: UserControlLink
    {
        private readonly PasswordBox m_PasswordBox;
        private readonly System.Windows.Controls.Image m_VisibilityImage;
        private bool m_ShowSecret = false;

        public PasswordUserControlLink(PasswordBox passwordBox, Button visibilityButton, System.Windows.Controls.Image visibilityImage)
        {
            m_PasswordBox = passwordBox;
            m_VisibilityImage = visibilityImage;
            visibilityButton.Click += VisibilityButton_Click;
        }

        private void VisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            m_ShowSecret = !m_ShowSecret;
            m_PasswordBox.HideChars = !m_ShowSecret;
            m_VisibilityImage.Source = new BitmapImage(new Uri((m_ShowSecret) ? @"/Assets/sight-enabled.png" : @"/Assets/sight-disabled.png", UriKind.Relative));
        }

        protected override void Load()
        {
            m_PasswordBox.Password = GetSettings();
        }

        protected override void Save()
        {
            SetSettings(m_PasswordBox.Password);
        }
    }
}
