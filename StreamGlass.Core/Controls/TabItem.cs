using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace StreamGlass.Core.Controls
{
    public class TabItem : System.Windows.Controls.TabItem, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<TabItem, string>("BrushPaletteKey", "background");
        [Description("The brush palette key of the tab item's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<TabItem, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the tab item's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<TabItem, string>("TranslationKey", string.Empty, (elem, key) => elem.UpdateTranslationKey(key));
        [Description("The translation key of the tab item's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        private TranslationKey m_TranslationKey = new(string.Empty);
        private void UpdateTranslationKey(string key) => m_TranslationKey = new(key);
        public void SetTranslationKey(TranslationKey key) => m_TranslationKey = key;

        public TabItem() => Translator.CurrentLanguageChanged += Translator_CurrentLanguageChanged;

        ~TabItem() => Translator.CurrentLanguageChanged -= Translator_CurrentLanguageChanged;

        private void Translator_CurrentLanguageChanged()
        {
            Dispatcher.Invoke(delegate
            {
                if (Translator.HaveKey(m_TranslationKey))
                    Header = m_TranslationKey.ToString();
            });
        }

        private void UpdateStyle(BrushPaletteManager palette)
        {
            Style style = new(typeof(TabItem));
            ControlTemplate template = new(typeof(TabItem));

            FrameworkElementFactory presenterFactory = new(typeof(ContentPresenter)) { Name = "ContentSite" };
            presenterFactory.SetValue(MarginProperty, new Thickness(10, 2, 10, 2));
            presenterFactory.SetValue(ContentPresenter.ContentSourceProperty, "Header");
            presenterFactory.SetValue(HorizontalAlignmentProperty, new TemplateBindingExtension(HorizontalContentAlignmentProperty));
            presenterFactory.SetValue(VerticalAlignmentProperty, new TemplateBindingExtension(VerticalContentAlignmentProperty));

            FrameworkElementFactory gridFactory = new(typeof(System.Windows.Controls.Grid)) { Name = "TabPanel" };
            gridFactory.AppendChild(presenterFactory);

            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border));
            //TODO: Style border
            elemFactory.AppendChild(gridFactory);
            template.VisualTree = elemFactory;

            if (palette.TryGetColor("background_2", out var color))
            {
                Trigger mouseOverTrigger = new() { Property = IsMouseOverProperty, Value = true };
                mouseOverTrigger.Setters.Add(new Setter() { TargetName = "TabPanel", Property = BackgroundProperty, Value = color! });

                Trigger selectedTrigger = new() { Property = IsSelectedProperty, Value = true };
                selectedTrigger.Setters.Add(new Setter() { TargetName = "TabPanel", Property = BackgroundProperty, Value = color! });

                template.Triggers.Add(mouseOverTrigger);
                template.Triggers.Add(selectedTrigger);
            }

            Setter templateSetter = new() { Property = Control.TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);
            Resources[typeof(TabItem)] = style;
        }

        public void Update(BrushPaletteManager palette)
        {
            UpdateStyle(palette);
            if (palette.TryGetColor(BrushPaletteKey, out var topBarBackground))
                Background = topBarBackground;
            if (palette.TryGetColor(TextBrushPaletteKey, out var topBarText))
                Foreground = topBarText;
            if (Content is IUIElement updatable)
                updatable.Update(palette);
        }
    }
}
