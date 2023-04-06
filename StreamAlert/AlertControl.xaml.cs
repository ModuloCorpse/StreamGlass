﻿using System;
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
    }
}