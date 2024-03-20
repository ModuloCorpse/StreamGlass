using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass.Core.Controls
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

        private TranslationKey m_TranslationKey = new(string.Empty);
        public void SetTranslationKey(TranslationKey key) => m_TranslationKey = key;

        public Label() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~Label() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

        private void Translator_CurrentLanguageChanged()
        {
            Dispatcher.Invoke(delegate
            {
                if (Translator.HaveKey(m_TranslationKey))
                    Text = m_TranslationKey.ToString();
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
