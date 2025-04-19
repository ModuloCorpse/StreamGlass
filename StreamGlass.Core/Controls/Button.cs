using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
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

        private TranslationKey m_TranslationKey = new(string.Empty);
        public void SetTranslationKey(TranslationKey key)
        {
            m_TranslationKey = key;
            Translator_CurrentLanguageChanged();
        }

        public Button() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~Button() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

        private void Translator_CurrentLanguageChanged()
        {
            Dispatcher.Invoke(delegate
            {
                if (Translator.HaveKey(m_TranslationKey))
                    Content = m_TranslationKey.ToString();
            });
        }

        public void Update(BrushPaletteManager palette)
        {
            Helper.ApplyButtonPalette<System.Windows.Controls.Button>(palette, this, BrushPaletteKey);
        }
    }
}
