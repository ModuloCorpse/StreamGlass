using StreamGlass.Core.Controls;
using CorpseLib.Wpf;

namespace StreamGlass.Twitch.Alerts
{
    public partial class AlertControl : UserControl
    {
        public AlertControl(VisualAlert alert, double contentFontSize)
        {
            InitializeComponent();
            AlertImage.Source = ImageLoader.LoadStaticImage(alert.ImagePath)?.Source;
            AlertMessage.SetText(alert.Message);
            AlertMessage.SetFontSize(contentFontSize);
            MessagePanel.BrushPaletteKey = "chat_background";
        }

        public double MessageFontSize { get => AlertMessage.FontSize; }

        public void SetMessageFontSize(double fontSize) => AlertMessage.SetFontSize(fontSize);
    }
}
