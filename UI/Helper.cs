using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StreamGlass.UI
{
    public static class Helper
    {
        public static void ApplyButtonPalette<T>(BrushPaletteManager palette, T button, string paletteKey) where T : ButtonBase
        {
            string overKey = string.Format("{0}_over", paletteKey);
            string borderKey = string.Format("{0}_border", paletteKey);
            string borderOverKey = string.Format("{0}_border_over", paletteKey);
            Style style = new(typeof(T), button.Style);
            if (palette.TryGetColor(paletteKey, out var paletteBrush))
            {
                Setter backgroundSetter = new() { Property = Control.BackgroundProperty, Value = paletteBrush };
                style.Setters.Add(backgroundSetter);
            }
            if (palette.TryGetColor(borderKey, out var borderPaletteBrush))
            {
                Setter borderSetter = new() { Property = Control.BorderBrushProperty, Value = borderPaletteBrush };
                style.Setters.Add(borderSetter);
            }
            ControlTemplate template = new(typeof(T));
            FrameworkElementFactory contentPresenterFactory = new(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border)) { Name = "Border" };
            elemFactory.SetValue(System.Windows.Controls.Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            elemFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = elemFactory;
            Setter templateSetter = new() { Property = Control.TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);
            if (palette.TryGetColor(overKey, out var overPaletteBrush))
            {
                Trigger trigger = new() { Property = UIElement.IsMouseOverProperty, Value = true };
                Setter overSetter = new() { Property = Control.BackgroundProperty, Value = overPaletteBrush };
                trigger.Setters.Add(overSetter);
                if (palette.TryGetColor(borderOverKey, out var overBorderPaletteBrush))
                {
                    Setter borderOverSetter = new() { Property = Control.BorderBrushProperty, Value = overBorderPaletteBrush };
                    trigger.Setters.Add(borderOverSetter);
                }
                style.Triggers.Add(trigger);
            }
            button.Style = style;
        }
    }
}
