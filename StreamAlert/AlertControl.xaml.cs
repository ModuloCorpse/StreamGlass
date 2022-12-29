using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.StreamChat;

namespace StreamGlass.StreamAlert
{
    public partial class AlertControl : UserControl
    {
        public AlertControl(IStreamChat streamChat, BrushPaletteManager palette, TranslationManager translation, Alert alert, double contentFontSize)
        {
            InitializeComponent();
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(alert.ImagePath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            AlertImage.Source = bitmap;
            AlertMessage.SetDisplayableMessage(streamChat, palette, alert.DisplayableMessage);
            AlertMessage.SetMessageFontSize(contentFontSize, false);
            MessagePanel.BrushPaletteKey = "chat_background";
            AlertMessage.SetPalette("chat_background", "chat_message");
            Update(palette, translation);
        }

        public double MessageFontSize { get => AlertMessage.FontSize; }

        public void SetMessageFontSize(double fontSize, bool updateEmotes = true)
        {
            AlertMessage.SetMessageFontSize(fontSize, updateEmotes);
        }

        internal void UpdateEmotes()
        {
            AlertMessage.UpdateEmotes();
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlertMessage.MessageContent.Width = (MessagePanel.ActualWidth - AlertImagePanel.ActualWidth) - 20;
            Height = AlertMessage.MessageContent.ActualHeight + Margin.Top + Margin.Bottom + AlertMessage.Margin.Top + AlertMessage.Margin.Bottom;
            if (Height < AlertImage.Height)
                Height = AlertImage.Height;
            AlertImagePanel.Height = Height;
            UpdateEmotes();
        }
    }
}
