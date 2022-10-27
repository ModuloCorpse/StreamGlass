using StreamGlass.Settings.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamGlass.Settings
{
    public class PasswordUserControlLink: UserControlLink
    {
        private bool m_ShowSecret = false;
        private readonly PasswordBox m_PasswordBox;
        private readonly TextBox m_TextBox;
        private readonly Image m_VisibilityImage;

        public PasswordUserControlLink(PasswordBox passwordBox, TextBox textBox, Button visibilityButton, Image visibilityImage)
        {
            m_PasswordBox = passwordBox;
            m_TextBox = textBox;
            m_VisibilityImage = visibilityImage;
            visibilityButton.Click += VisibilityButton_Click;
        }

        private void VisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            if (!m_ShowSecret)
            {
                m_TextBox.Text = m_PasswordBox.Password;
                m_PasswordBox.Visibility = Visibility.Collapsed;
                m_TextBox.Visibility = Visibility.Visible;
                m_VisibilityImage.Source = new BitmapImage(new Uri(@"/Assets/sight-enabled.png", UriKind.Relative));
            }
            else
            {
                m_PasswordBox.Password = m_TextBox.Text;
                m_PasswordBox.Visibility = Visibility.Visible;
                m_TextBox.Visibility = Visibility.Collapsed;
                m_VisibilityImage.Source = new BitmapImage(new Uri(@"/Assets/sight-disabled.png", UriKind.Relative));
            }
            m_ShowSecret = !m_ShowSecret;
        }

        internal override void Load()
        {
            m_PasswordBox.Password = GetSettings();
            m_TextBox.Text = GetSettings();
        }

        internal override void Save()
        {
            if (m_ShowSecret)
                SetSettings(m_TextBox.Text);
            else
                SetSettings(m_PasswordBox.Password);
        }
    }
}
