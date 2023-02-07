using System;
using System.Windows;
using System.Windows.Media.Imaging;
using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Connections;

namespace StreamGlass.StreamAlert
{
    public partial class AlertControl : UserControl
    {
        public AlertControl(ConnectionManager connectionManager, BrushPaletteManager palette, TranslationManager translation, Alert alert, double contentFontSize)
        {
            InitializeComponent();
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(alert.ImagePath, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            AlertImage.Source = bitmap;
            AlertMessage.SetText(alert.Message);
            AlertMessage.SetFontSize(contentFontSize);
            MessagePanel.BrushPaletteKey = "chat_background";
            Update(palette, translation);
        }

        public double MessageFontSize { get => AlertMessage.FontSize; }

        public void SetMessageFontSize(double fontSize) => AlertMessage.SetFontSize(fontSize);

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlertMessage.Width = (MessagePanel.ActualWidth - AlertImagePanel.ActualWidth) - 20;
            Height = AlertMessage.ActualHeight + Margin.Top + Margin.Bottom + AlertMessage.Margin.Top + AlertMessage.Margin.Bottom;
            if (Height < AlertImage.Height)
                Height = AlertImage.Height;
            AlertImagePanel.Height = Height;
        }
    }
}
