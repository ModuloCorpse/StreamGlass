using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using StreamGlass.Core.Controls;

namespace StreamGlass.StreamAlert
{
    public partial class AlertEditor : Dialog
    {
        private AlertScrollPanel.AlertInfo? m_CreatedAlertInfo = null;

        internal AlertEditor(Core.Controls.Window parent, AlertScrollPanel.AlertInfo alertInfo) : base(parent)
        {
            InitializeComponent();
            AlertEnableCheckBox.IsChecked = alertInfo.IsEnabled;
            ChatMessageEnableCheckBox.IsChecked = alertInfo.HaveChatMessage;
            AlertImageTextBox.Text = alertInfo.ImgPath;
            AlertContentTextBox.Text = alertInfo.Prefix;
            ChatMessageContentTextBox.Text = alertInfo.ChatMessage;
        }

        internal AlertScrollPanel.AlertInfo? AlertInfo => m_CreatedAlertInfo;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedAlertInfo = new(AlertImageTextBox.Text, AlertContentTextBox.Text, ChatMessageContentTextBox.Text, AlertEnableCheckBox.IsChecked ?? false, ChatMessageEnableCheckBox.IsChecked ?? false);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedAlertInfo = null;
            Close();
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
