using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
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

        private TranslationKey m_TranslationKey = new(string.Empty);
        public void SetTranslationKey(TranslationKey key)
        {
            m_TranslationKey = key;
            Translator_CurrentLanguageChanged();
        }

        public RepeatButton() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~RepeatButton() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

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
            Helper.ApplyButtonPalette<System.Windows.Controls.Primitives.RepeatButton>(palette, this, BrushPaletteKey);
        }
    }
}
