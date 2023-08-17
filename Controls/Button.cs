using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace StreamGlass.Controls
{
    public class Button : System.Windows.Controls.Button, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<Button, string>("BrushPaletteKey", "button");
        [Description("The brush palette key of the button"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<Button, string>("TranslationKey", "");
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
            Helper.ApplyButtonPalette<System.Windows.Controls.Button>(palette, this, BrushPaletteKey);
        }
    }
}
