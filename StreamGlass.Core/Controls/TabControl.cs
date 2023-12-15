using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class TabControl : System.Windows.Controls.TabControl, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<TabControl, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the tab control's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<TabControl, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the tab control's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var topBarBackground))
                Background = topBarBackground;
            if (palette.TryGetColor(TextBrushPaletteKey, out var topBarText))
                Foreground = topBarText;
            foreach (object item in Items)
            {
                if (item is TabItem child)
                    child.Update(palette);
            }
        }
    }
}
