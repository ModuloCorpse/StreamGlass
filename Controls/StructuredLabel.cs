using CorpseLib.StructuredText;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamGlass.Controls
{
    public class StructuredLabel : RichTextBox, IUIElement
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
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty("TextBrushPaletteKey", "text", (StructuredLabel instance, string value) => instance.Property_SetTextBrushPaletteKey(value));
        [Description("The brush palette key of the structured label's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        internal void Property_SetTextBrushPaletteKey(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
            {
                foreach (SectionRun sectionRun in m_Text)
                    sectionRun.BrushPaletteKey = source;
            }
        }
        #endregion TextBrushPaletteKey

        private readonly List<SectionRun> m_Text = [];
        private readonly List<Image> m_Images = [];
        private double m_FontSize = 14;

        public void SetText(Text text)
        {
            System.Windows.Documents.Paragraph paragraph = new()
            {
                Margin = new(0),
                Padding = new(0),
                TextIndent = 0
            };

            foreach (Section section in text)
            {
                if (section.SectionType == Section.Type.TEXT)
                {
                    SectionRun textInline = new(section);
                    paragraph.Inlines.Add(textInline);
                    m_Text.Add(textInline);
                }
                else if (section.SectionType == Section.Type.IMAGE)
                {
                    string emoteURL = section.Content;
                    BitmapImage bitmap = new();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(emoteURL, UriKind.Absolute);
                    bitmap.EndInit();
                    Image emoteImage = new()
                    {
                        Width = m_FontSize * 1.5,
                        Height = m_FontSize * 1.5,
                        Source = bitmap
                    };
                    System.Windows.Documents.InlineUIContainer emoteInline = new()
                    {
                        BaselineAlignment = BaselineAlignment.Center,
                        Child = emoteImage
                    };
                    paragraph.Inlines.Add(emoteInline);
                    m_Images.Add(emoteImage);
                }
            }

            Document.PagePadding = new(0);
            Document.Blocks.Clear();
            Document.Blocks.Add(paragraph);
        }

        public void SetFontSize(double fontSize)
        {
            m_FontSize = fontSize;
            foreach (SectionRun sectionRun in m_Text)
                sectionRun.SetFontSize(fontSize);
            foreach (Image image in m_Images)
            {
                image.Width = fontSize * 1.5;
                image.Height = fontSize * 1.5;
            }
        }

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            foreach (SectionRun sectionRun in m_Text)
                sectionRun.Update(palette);
        }
    }
}
