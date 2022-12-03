using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class Menu : System.Windows.Controls.Menu, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(Menu), new FrameworkPropertyMetadata("background"));
        [Description("The brush palette key of the menu's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = DependencyProperty.Register("TextBrushPaletteKey", typeof(string), typeof(Menu), new FrameworkPropertyMetadata("text"));
        [Description("The brush palette key of the menu's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var topBarBackground))
                Background = topBarBackground;
            if (palette.TryGetColor(TextBrushPaletteKey, out var topBarText))
                Foreground = topBarText;
            foreach (System.Windows.Controls.MenuItem item in Items)
            {
                if (item is MenuItem child)
                    child.Update(palette, translation);
            }
        }
    }
}
