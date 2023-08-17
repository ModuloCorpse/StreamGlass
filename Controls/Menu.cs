using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
{
    public class Menu : System.Windows.Controls.Menu, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<Menu, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the menu's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<Menu, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the menu's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var topBarBackground))
            {
                Background = topBarBackground;
                BorderBrush = topBarBackground;
            }
            if (palette.TryGetColor(TextBrushPaletteKey, out var topBarText))
                Foreground = topBarText;
            foreach (object item in Items)
            {
                if (item is MenuItem child)
                    child.Update(palette);
            }
        }
    }
}
