﻿using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Core.Controls
{
    public class MenuItem : System.Windows.Controls.MenuItem, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<MenuItem, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the menu item's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<MenuItem, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the menu item's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<MenuItem, string>("TranslationKey", string.Empty, (elem, key) => elem.UpdateTranslationKey(key));
        [Description("The translation key of the menu item's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        private TranslationKey m_TranslationKey = new(string.Empty);
        private void UpdateTranslationKey(string key) => m_TranslationKey = new(key);
        public void SetTranslationKey(TranslationKey key) => m_TranslationKey = key;

        public MenuItem() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~MenuItem() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

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
            if (palette.TryGetColor(BrushPaletteKey, out var topBarBackground))
            {
                Background = topBarBackground;
                BorderBrush = topBarBackground;
            }
            if (palette.TryGetColor(TextBrushPaletteKey, out var topBarText))
                Foreground = topBarText;
            foreach (object item in Items)
            {
                if (item is MenuItem child)
                    child.Update(palette);
            }
        }
    }
}
