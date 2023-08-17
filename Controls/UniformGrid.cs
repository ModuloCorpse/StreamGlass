using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
{
    public class UniformGrid : System.Windows.Controls.Primitives.UniformGrid, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<UniformGrid, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the grid"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            foreach (UIElement element in Children)
            {
                if (element is IUIElement updatable)
                    updatable.Update(palette);
            }
        }
    }
}
