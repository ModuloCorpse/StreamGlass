using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class PasswordBox : CorpseLib.Wpf.PasswordBox, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<PasswordBox, string>("BrushPaletteKey", "background_2");
        [Description("The brush palette key of the password box's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<PasswordBox, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the password box's text"), Category("Common Properties")]
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
        }
    }
}
