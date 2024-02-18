using StreamGlass.Core.Controls;
using CorpseLib.Wpf;

namespace StreamGlass.Twitch.Alert
{
    public partial class AlertControl : UserControl
    {
        public AlertControl(Alert alert, double contentFontSize)
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
