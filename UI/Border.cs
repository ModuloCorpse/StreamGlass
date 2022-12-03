using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class Border : System.Windows.Controls.Border, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(Border), new FrameworkPropertyMetadata("border"));
        [Description("The brush palette key of the border"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                BorderBrush = background;
            if (Child is IUIElement updatable)
                updatable.Update(palette, translation);
        }
    }
}
