using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
{
    public class RepeatButton : System.Windows.Controls.Primitives.RepeatButton, IUIElement
    {
        #region Value
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<RepeatButton, string>("BrushPaletteKey", "button");
        [Description("The brush palette key of the button"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion Value

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<RepeatButton, string>("TranslationKey", string.Empty);
        [Description("The translation key of the button's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public void Update(BrushPaletteManager palette)
        {
            if (Translator.HaveKey(TranslationKey))
                Content = Translator.Translate("${" + TranslationKey + "}");
            Helper.ApplyButtonPalette<System.Windows.Controls.Primitives.RepeatButton>(palette, this, BrushPaletteKey);
        }
    }
}
