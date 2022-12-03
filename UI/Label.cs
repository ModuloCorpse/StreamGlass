using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass.UI
{
    public class Label : TextBlock, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(Label), new FrameworkPropertyMetadata("text"));
        [Description("The brush palette key of the label's text"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = DependencyProperty.Register("TranslationKey", typeof(string), typeof(Label), new FrameworkPropertyMetadata(""));
        [Description("The translation key of the label's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (translation.TryGetTranslation(TranslationKey, out var value))
                Text = value;
            if (palette.TryGetColor(BrushPaletteKey, out var foreground))
                Foreground = foreground;
        }
    }
}
