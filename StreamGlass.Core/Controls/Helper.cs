using CorpseLib.ManagedObject;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StreamGlass.Core.Controls
{
    public static class Helper
    {
        public delegate void PropertyChangedDelegate<TUIElement, TPropertyType>(TUIElement instance, TPropertyType value);
        public static DependencyProperty NewProperty<TUIElement, TPropertyType>(string name, TPropertyType defaultValue, PropertyChangedDelegate<TUIElement, TPropertyType> callback) where TUIElement : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPropertyType), typeof(TUIElement),
                new PropertyMetadata(defaultValue, new PropertyChangedCallback((DependencyObject d, DependencyPropertyChangedEventArgs e) => callback((TUIElement)d, (TPropertyType)e.NewValue))));
        }
        public static DependencyProperty NewProperty<TUIElement, TPropertyType>(string name, TPropertyType defaultValue) where TUIElement : DependencyObject
        {
            return DependencyProperty.Register(name, typeof(TPropertyType), typeof(TUIElement), new PropertyMetadata(defaultValue));
        }

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

        public static Style GetScrollBarRepeatButtonStyle()
        {
            Style style = new(typeof(System.Windows.Controls.Primitives.RepeatButton));
            Setter overrideSetter = new() { Property = System.Windows.Controls.Control.OverridesDefaultStyleProperty, Value = true };
            style.Setters.Add(overrideSetter);
            Setter backgroundSetter = new() { Property = System.Windows.Controls.Control.BackgroundProperty, Value = Brushes.Transparent };
            style.Setters.Add(backgroundSetter);
            Setter focusableSetter = new() { Property = System.Windows.Controls.Control.FocusableProperty, Value = false };
            style.Setters.Add(focusableSetter);
            Setter tabStopSetter = new() { Property = System.Windows.Controls.Control.IsTabStopProperty, Value = false };
            style.Setters.Add(tabStopSetter);
            ControlTemplate template = new(typeof(System.Windows.Controls.Primitives.RepeatButton));
            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border)) { Name = "Border" };
            elemFactory.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(10));
            elemFactory.SetValue(System.Windows.Controls.Control.WidthProperty, new TemplateBindingExtension(System.Windows.Controls.Control.WidthProperty));
            elemFactory.SetValue(System.Windows.Controls.Control.HeightProperty, new TemplateBindingExtension(System.Windows.Controls.Control.HeightProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, Brushes.Transparent);
            template.VisualTree = elemFactory;
            Setter templateSetter = new() { Property = System.Windows.Controls.Control.TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);
            return style;
        }

        public static Style GetScrollBarThumbStyle(Brush brush)
        {
            Style style = new(typeof(Thumb));
            Setter overrideSetter = new() { Property = System.Windows.Controls.Control.OverridesDefaultStyleProperty, Value = true };
            style.Setters.Add(overrideSetter);
            Setter tabStopSetter = new() { Property = System.Windows.Controls.Control.IsTabStopProperty, Value = false };
            style.Setters.Add(tabStopSetter);
            ControlTemplate template = new(typeof(Thumb));
            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Border)) { Name = "Border" };
            elemFactory.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(8));
            elemFactory.SetValue(System.Windows.Controls.Border.WidthProperty, new TemplateBindingExtension(System.Windows.Controls.Control.WidthProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.HeightProperty, new TemplateBindingExtension(System.Windows.Controls.Control.HeightProperty));
            elemFactory.SetValue(System.Windows.Controls.Border.SnapsToDevicePixelsProperty, true);
            elemFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, brush);
            template.VisualTree = elemFactory;
            Setter templateSetter = new() { Property = System.Windows.Controls.Control.TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);
            return style;
        }

        public static void FillComboBox<T>(Manager<T> manager, ref ComboBox comboBox, bool addEmpty = true) where T : Object<T>
        {
            comboBox.Items.Clear();
            if (addEmpty)
            {
                Object<T>.Info info = new(string.Empty, string.Empty);
                comboBox.Items.Add(info);
                if (manager.CurrentObject == null)
                    comboBox.SelectedItem = info;
            }
            foreach (T obj in manager.Objects)
            {
                Object<T>.Info objectInfo = obj.ObjectInfo;
                comboBox.Items.Add(objectInfo);
                if (objectInfo.ID == manager.CurrentObjectID)
                    comboBox.SelectedItem = objectInfo;
            }
        }
    }
}
