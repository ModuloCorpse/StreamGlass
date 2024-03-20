using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class GroupBox : System.Windows.Controls.GroupBox, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<GroupBox, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the group box's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<GroupBox, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the group box's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        private TranslationKey m_TranslationKey = new(string.Empty);
        public void SetTranslationKey(TranslationKey key) => m_TranslationKey = key;

        public GroupBox() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~GroupBox() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

        private void Translator_CurrentLanguageChanged()
        {
            Dispatcher.Invoke(delegate
            {
                if (Translator.HaveKey(m_TranslationKey))
                    Header = m_TranslationKey.ToString();
            });
        }

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
            if (Content is IUIElement updatable)
                updatable.Update(palette);
        }
    }
}
