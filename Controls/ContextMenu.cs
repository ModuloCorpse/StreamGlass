using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
{
    public class ContextMenu : System.Windows.Controls.ContextMenu, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<ContextMenu, string>("BrushPaletteKey", "string");
        [Description("The brush palette key of the context menu's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<ContextMenu, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the context menu's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
            foreach (System.Windows.Controls.MenuItem element in Items)
            {
                if (element is IUIElement updatable)
                    updatable.Update(palette);
            }
        }
    }
}
