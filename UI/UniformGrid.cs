using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class UniformGrid : System.Windows.Controls.Primitives.UniformGrid, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(UniformGrid), new FrameworkPropertyMetadata("background"));
        [Description("The brush palette key of the grid"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            foreach (UIElement element in Children)
            {
                if (element is IUIElement updatable)
                    updatable.Update(palette, translation);
            }
        }
    }
}
