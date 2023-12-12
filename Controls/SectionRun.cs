using CorpseLib.StructuredText;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace StreamGlass.Controls
{
    public class SectionRun : System.Windows.Documents.Run, IUIElement
    {
        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<SectionRun, string>("BrushPaletteKey", "text");
        [Description("The brush palette key of the structured text box"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        private readonly Dictionary<string, object?> m_Style = [];

        public bool IsStyleOverriden => m_Style != null;

        public SectionRun(Section section)
        {
            Text = section.Content;
            m_Style = section.Properties;

            if (m_Style.TryGetValue("FontSize", out object? fontSize))
                FontSize = (double)fontSize!;
        }

        public void SetFontSize(double fontSize)
        {
            if (!m_Style.ContainsKey("FontSize"))
                FontSize = fontSize;
        }

        public void Update(BrushPaletteManager palette)
        {
            if (m_Style.TryGetValue("ForegroundPaletteKey", out object? foregroundPaletteKey))
            {
                if (palette.TryGetColor((string)foregroundPaletteKey!, out var foreground))
                    Foreground = foreground;
            }
            else if (palette.TryGetColor(BrushPaletteKey, out var foreground))
                Foreground = foreground;

            if (m_Style.TryGetValue("BackgroundPaletteKey", out object? backgroundPaletteKey) && palette.TryGetColor((string)backgroundPaletteKey!, out var background))
                Background = background;
        }
    }
}
