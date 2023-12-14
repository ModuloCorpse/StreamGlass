using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass.Controls
{
    public class Label : TextBlock, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<Label, string>("BrushPaletteKey", "text");
        [Description("The brush palette key of the label's text"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<Label, string>("TranslationKey", string.Empty);
        [Description("The translation key of the label's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public Label() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~Label() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

        private void Translator_CurrentLanguageChanged()
        {
            Dispatcher.Invoke(delegate
            {
                if (Translator.HaveKey(TranslationKey))
                    Text = Translator.Translate("${" + TranslationKey + "}");
            });
        }

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var foreground))
                Foreground = foreground;
            if (ContextMenu is IUIElement menu)
                menu.Update(palette);
        }
    }
}
