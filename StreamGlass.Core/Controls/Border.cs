using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class Border : System.Windows.Controls.Border, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<Border, string>("BrushPaletteKey", "border");
        [Description("The brush palette key of the border"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                BorderBrush = background;
            if (Child is IUIElement updatable)
                updatable.Update(palette);
        }
    }
}
