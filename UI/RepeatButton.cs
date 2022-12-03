using System.ComponentModel;
using System.Windows;

namespace StreamGlass.UI
{
    public class RepeatButton : System.Windows.Controls.Primitives.RepeatButton, IUIElement
    {
        #region Value
        public static readonly DependencyProperty BrushPaletteKeyProperty = DependencyProperty.Register("BrushPaletteKey", typeof(string), typeof(RepeatButton), new FrameworkPropertyMetadata("button"));
        [Description("The brush palette key of the button"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion Value

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = DependencyProperty.Register("TranslationKey", typeof(string), typeof(RepeatButton), new FrameworkPropertyMetadata(""));
        [Description("The translation key of the button's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (translation.TryGetTranslation(TranslationKey, out var value))
                Content = value;
            Helper.ApplyButtonPalette<System.Windows.Controls.Primitives.RepeatButton>(palette, this, BrushPaletteKey);
        }
    }
}
