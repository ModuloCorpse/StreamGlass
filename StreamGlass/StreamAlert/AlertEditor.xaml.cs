using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using StreamGlass.Audio;
using StreamGlass.Core.Controls;

namespace StreamGlass.StreamAlert
{
    public partial class AlertEditor : Dialog
    {
        private AlertScrollPanel.AlertInfo? m_CreatedAlertInfo = null;
        private Sound? m_CreatedSound = null;

        internal AlertEditor(Core.Controls.Window parent, AlertScrollPanel.AlertInfo alertInfo) : base(parent)
        {
            InitializeComponent();
            AlertEnableCheckBox.IsChecked = alertInfo.IsEnabled;
            ChatMessageEnableCheckBox.IsChecked = alertInfo.HaveChatMessage;
            AlertImageTextBox.Text = alertInfo.ImgPath;
            AlertContentTextBox.Text = alertInfo.Prefix;
            ChatMessageContentTextBox.Text = alertInfo.ChatMessage;
            if (alertInfo.Audio != null)
            {
                m_CreatedSound = alertInfo.Audio;
                AlertAudioFileLabel.Text = Path.GetFileName(alertInfo.Audio.File);
            }
        }

        internal AlertScrollPanel.AlertInfo? AlertInfo => m_CreatedAlertInfo;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedAlertInfo = new(m_CreatedSound, AlertImageTextBox.Text, AlertContentTextBox.Text, ChatMessageContentTextBox.Text, AlertEnableCheckBox.IsChecked ?? false, ChatMessageEnableCheckBox.IsChecked ?? false);
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedAlertInfo = null;
            m_CreatedSound = null;
            OnCancelClick();
        }

        private void AlertImageFileDialog_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                InitialDirectory = "C:\\",
                Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
                AlertImageTextBox.Text = openFileDialog.FileName;
        }

        private void AlertAudioFileDialog_Click(object sender, RoutedEventArgs e)
        {
            SoundEditor openFileDialog = new(this, m_CreatedSound);
            if (openFileDialog.ShowDialog() == true)
            {
                m_CreatedSound = openFileDialog.Sound;
                if (m_CreatedSound != null)
                    AlertAudioFileLabel.Text = Path.GetFileName(m_CreatedSound.File);
            }
        }

        private void AlertAudioTest_Click(object sender, RoutedEventArgs e)
        {
            if (m_CreatedSound != null)
                SoundManager.PlaySound(m_CreatedSound);
        }

        private void AlertImageTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(AlertImageTextBox.Text, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            AlertImage.Source = bitmap;
        }
    }
}
