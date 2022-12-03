using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StreamGlass.UI
{
    public class ScrollViewer : System.Windows.Controls.ScrollViewer, IUIElement
    {
        public ScrollViewer(): base()
        {
            Style style = new(typeof(System.Windows.Controls.Primitives.RepeatButton));
            Setter overrideSetter = new() { Property = OverridesDefaultStyleProperty, Value = true };
            style.Setters.Add(overrideSetter);
            Setter backgroundSetter = new() { Property = BackgroundProperty, Value = Brushes.Transparent };
            style.Setters.Add(backgroundSetter);
            Setter focusableSetter = new() { Property = FocusableProperty, Value = false };
            style.Setters.Add(focusableSetter);
            Setter tabStopSetter = new() { Property = IsTabStopProperty, Value = false };
            style.Setters.Add(tabStopSetter);
            ControlTemplate template = new(typeof(System.Windows.Controls.Primitives.RepeatButton));
            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border)) { Name = "Border" };
            elemFactory.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(10));
            elemFactory.SetValue(WidthProperty, new TemplateBindingExtension(WidthProperty));
            elemFactory.SetValue(HeightProperty, new TemplateBindingExtension(HeightProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, Brushes.Transparent);
            template.VisualTree = elemFactory;
            Setter templateSetter = new() { Property = TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);
            Resources["RepeatButtonTransparent"] = style;
        }

        public void Update(BrushPaletteManager palette, TranslationManager translation)
        {
            if (palette.TryGetColor("chat_scrollbar", out var scrollbarBrush))
            {
                Style style = new(typeof(Thumb));
                Setter overrideSetter = new() { Property = OverridesDefaultStyleProperty, Value = true };
                style.Setters.Add(overrideSetter);
                Setter tabStopSetter = new() { Property = IsTabStopProperty, Value = false };
                style.Setters.Add(tabStopSetter);
                ControlTemplate template = new(typeof(Thumb));
                FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border)) { Name = "Border" };
                elemFactory.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));
                elemFactory.SetValue(System.Windows.Controls.Border.WidthProperty, new TemplateBindingExtension(WidthProperty));
                elemFactory.SetValue(System.Windows.Controls.Border.HeightProperty, new TemplateBindingExtension(HeightProperty));
                elemFactory.SetValue(System.Windows.Controls.Border.SnapsToDevicePixelsProperty, true);
                elemFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, scrollbarBrush);
                template.VisualTree = elemFactory;
                Setter templateSetter = new() { Property = TemplateProperty, Value = template };
                style.Setters.Add(templateSetter);
                Resources["ScrollBarThumbVertical"] = style;
            }
            if (Content is IUIElement updatable)
                updatable.Update(palette, translation);
        }
    }
}
