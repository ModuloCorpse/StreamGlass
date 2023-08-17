using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace StreamGlass.Controls
{
    public class ScrollViewer : System.Windows.Controls.ScrollViewer, IUIElement
    {
        private void InitStyle()
        {
            Style style = new(typeof(ScrollBar));
            Setter backgroundSetter = new() { Property = BackgroundProperty, Value = Brushes.Transparent };
            Setter borderBrushSetter = new() { Property = BorderBrushProperty, Value = Brushes.Transparent };
            style.Setters.Add(backgroundSetter);
            style.Setters.Add(borderBrushSetter);
            ControlTemplate template = new(typeof(ScrollBar));

            FrameworkElementFactory elemFactory = new(typeof(System.Windows.Controls.Grid));
            elemFactory.SetValue(SnapsToDevicePixelsProperty, true);

            var row1 = new FrameworkElementFactory(typeof(RowDefinition));
            row1.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
            elemFactory.AppendChild(row1);

            var row2 = new FrameworkElementFactory(typeof(RowDefinition));
            row2.SetValue(RowDefinition.HeightProperty, new GridLength(0.00001, GridUnitType.Star));
            elemFactory.AppendChild(row2);

            var row3 = new FrameworkElementFactory(typeof(RowDefinition));
            row3.SetValue(RowDefinition.HeightProperty, new GridLength(1, GridUnitType.Auto));
            elemFactory.AppendChild(row3);

            FrameworkElementFactory trackFactory = new(typeof(Track)) { Name = "PART_Track" };
            trackFactory.SetValue(System.Windows.Controls.Primitives.Track.IsDirectionReversedProperty, true);
            trackFactory.SetValue(FrameworkElement.IsEnabledProperty, new TemplateBindingExtension(Control.IsMouseOverProperty));
            trackFactory.SetValue(Grid.RowProperty, 1);
            elemFactory.AppendChild(trackFactory);

            FrameworkElementFactory borderFactory = new(typeof(System.Windows.Controls.Border));
            borderFactory.SetValue(System.Windows.Controls.Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            borderFactory.SetValue(System.Windows.Controls.Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            borderFactory.SetValue(System.Windows.Controls.Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            borderFactory.SetValue(Grid.RowProperty, 1);
            elemFactory.AppendChild(borderFactory);

            template.VisualTree = elemFactory;
            Setter templateSetter = new() { Property = Control.TemplateProperty, Value = template };
            style.Setters.Add(templateSetter);

            Resources[typeof(ScrollBar)] = style;
        }

        public ScrollViewer(): base()
        {
            InitStyle();
        }

        public void Update(BrushPaletteManager palette)
        {
            if (Content is IUIElement updatable)
                updatable.Update(palette);

            System.Windows.Controls.Primitives.ScrollBar? scrollBar = Template.FindName("PART_VerticalScrollBar", this)! as System.Windows.Controls.Primitives.ScrollBar;
            System.Windows.Controls.Primitives.Track? track = scrollBar?.Template.FindName("PART_Track", scrollBar)! as System.Windows.Controls.Primitives.Track;
            if (track is IUIElement updatableTrack)
                updatableTrack.Update(palette);
        }
    }
}
