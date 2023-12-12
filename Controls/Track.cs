namespace StreamGlass.Controls
{
    public class Track : System.Windows.Controls.Primitives.Track, IUIElement
    {
        public Track()
        {
            DecreaseRepeatButton = new System.Windows.Controls.Primitives.RepeatButton
            {
                Style = Helper.GetScrollBarRepeatButtonStyle()
            };
            IncreaseRepeatButton = new System.Windows.Controls.Primitives.RepeatButton
            {
                Style = Helper.GetScrollBarRepeatButtonStyle()
            };
            Thumb = new System.Windows.Controls.Primitives.Thumb();
        }

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor("chat_scrollbar", out var scrollbarBrush))
                Thumb.Style = Helper.GetScrollBarThumbStyle(scrollbarBrush);
        }
    }
}
