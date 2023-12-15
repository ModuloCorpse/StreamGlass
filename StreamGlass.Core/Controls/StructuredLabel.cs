using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;

namespace StreamGlass.Core.Controls
{
    public class StructuredLabel : CorpseLib.Wpf.StructuredLabel, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<StructuredLabel, string>("BrushPaletteKey", "chat_background");
        [Description("The brush palette key of the structured label"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<StructuredLabel, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the structured label's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            foreach (var block in Document.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is CorpseLib.Wpf.SectionRun sectionRun)
                        {
                            if (sectionRun.TryGetStyle("ForegroundPaletteKey", out string? foregroundPaletteKey))
                            {
                                if (palette.TryGetColor(foregroundPaletteKey!, out var foreground))
                                    sectionRun.Foreground = foreground;
                            }
                            else if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                                sectionRun.Foreground = foreground;

                            if (sectionRun.TryGetStyle("BackgroundPaletteKey", out string? backgroundPaletteKey))
                            {
                                if (palette.TryGetColor(backgroundPaletteKey!, out var runBackground))
                                    sectionRun.Background = runBackground;
                            }
                        }
                    }
                }
            }
        }
    }
}
