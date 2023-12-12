using CorpseLib.Translation;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
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

        #region TranslationKey
        public static readonly DependencyProperty TranslationKeyProperty = Helper.NewProperty<GroupBox, string>("TranslationKey", string.Empty);
        [Description("The translation key of the group box's text"), Category("Common Properties")]
        public string TranslationKey
        {
            get => (string)GetValue(TranslationKeyProperty);
            set => SetValue(TranslationKeyProperty, value);
        }
        #endregion TranslationKey

        public void Update(BrushPaletteManager palette)
        {
            if (Translator.HaveKey(TranslationKey))
                Header = Translator.Translate("${" + TranslationKey + "}");
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
            if (Content is IUIElement updatable)
                updatable.Update(palette);
        }
    }
}
