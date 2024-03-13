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

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<Button, string>("TranslationKey", string.Empty, (elem, key) => elem.UpdateTranslationKey(key));
        [Description("The translation key of the button's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        private TranslationKey m_TranslationKey = new(string.Empty);
        private void UpdateTranslationKey(string key) => m_TranslationKey = new(key);
        public void SetTranslationKey(TranslationKey key) => m_TranslationKey = key;

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
