using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class StackPanel : System.Windows.Controls.StackPanel, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(StackPanel), new FrameworkPropertyMetadata("background"));
        [Description("The brush palette key of the stack panel"), Category("Common Properties")]
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
