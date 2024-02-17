using StreamGlass.Core.Controls;
using StreamGlass.Core.Connections;
using CorpseLib.Wpf;

namespace StreamGlass.Twitch.Alert
{
    public partial class AlertControl : UserControl
    {
        public AlertControl(ConnectionManager connectionManager, BrushPaletteManager palette, Alert alert, double contentFontSize)
        {
            InitializeComponent();
            AlertImage.Source = ImageLoader.LoadStaticImage(alert.ImagePath)?.Source;
            AlertMessage.SetText(alert.Message);
            AlertMessage.SetFontSize(contentFontSize);
            MessagePanel.BrushPaletteKey = "chat_background";
            Update(palette);
        }

        public double MessageFontSize { get => AlertMessage.FontSize; }

        public void SetMessageFontSize(double fontSize) => AlertMessage.SetFontSize(fontSize);
    }
}
