using StreamGlass.Core;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamGlass.Moderation
{
    public partial class HeldMessage : Core.Controls.UserControl
    {
        private readonly HeldMessageScrollPanel m_Parent;
        private readonly UserMessage m_HeldMessage;
        private double m_MaxFontSize;

        internal string ID => m_HeldMessage.ID;

        private static double GetFontSize(TextBlock textBlock, double textBlockFontSize)
        {
            Typeface typeFace = new(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch);
            Brush foreground = textBlock.Foreground;
            double dpi = VisualTreeHelper.GetDpi(textBlock).PixelsPerDip;
            double fontSize = textBlockFontSize;
            FormattedText ft = new(textBlock.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, fontSize, foreground, dpi);
            while (textBlock.Width < ft.Width)
            {
                fontSize -= 1;
                if (fontSize < 0)
                    return textBlockFontSize;
                ft = new(textBlock.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeFace, fontSize, foreground, dpi);
            }
            return fontSize;
        }

        public HeldMessage(HeldMessageScrollPanel parent, UserMessage message, double senderWidth, double senderFontSize, double fontSize)
        {
            m_Parent = parent;
            m_HeldMessage = message;
            InitializeComponent();
            HeldMessageLabel.SetText(message.Message);
            HeldMessageLabel.SetFontSize(fontSize);
            HeldSenderLabel.Text = message.UserDisplayName;
            HeldSenderLabel.Width = senderWidth;
            SetSenderNameFontSize(senderFontSize);
            BrushConverter converter = new();
            if (!string.IsNullOrWhiteSpace(message.Color))
            {
                SolidColorBrush? color = (SolidColorBrush?)converter.ConvertFrom(message.Color);
                if (color != null)
                    HeldSenderLabel.Foreground = color;
            }
        }

        public double MessageFontSize { get => HeldMessageLabel.FontSize; }

        public void SetSenderNameWidth(double width)
        {
            HeldSenderLabel.Width = width;
            HeldSenderLabel.FontSize = GetFontSize(HeldSenderLabel, m_MaxFontSize);
        }

        public void SetSenderNameFontSize(double fontSize)
        {
            m_MaxFontSize = fontSize;
            HeldSenderLabel.FontSize = GetFontSize(HeldSenderLabel, fontSize);
        }

        public void SetMessageFontSize(double fontSize) => HeldMessageLabel.FontSize = fontSize;

        private void AllowButton_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassCanals.ALLOW_MESSAGE.Emit(new(m_HeldMessage.Sender, m_HeldMessage.ID, true));
            m_Parent.Remove(this);
        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            StreamGlassCanals.ALLOW_MESSAGE.Emit(new(m_HeldMessage.Sender, m_HeldMessage.ID, false));
            m_Parent.Remove(this);
        }
    }
}
