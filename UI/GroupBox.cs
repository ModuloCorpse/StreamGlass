using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class GroupBox : System.Windows.Controls.GroupBox, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(GroupBox), new FrameworkPropertyMetadata("background"));
        [Description("The brush palette key of the group box's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = DependencyProperty.Register("TextBrushPaletteKey", typeof(string), typeof(GroupBox), new FrameworkPropertyMetadata("text"));
        [Description("The brush palette key of the group box's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = DependencyProperty.Register("TranslationKey", typeof(string), typeof(GroupBox), new FrameworkPropertyMetadata(""));
        [Description("The translation key of the group box's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (translation.TryGetTranslation(TranslationKey, out var value))
                Header = value;
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
            if (Content is IUIElement updatable)
                updatable.Update(palette, translation);
        }
    }
}
