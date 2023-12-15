using System.Windows.Media;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace StreamGlass.Core.Controls
{
    //TODO Switch to XAML
    //TODO Create ToggleSwitch control
    public class ToggleSwitch : ToggleButton
    {
        private static CornerRadius _defaultCornerRadius = new(0.0);
        private static readonly Brush _defaultOnColor = Brushes.MediumSeaGreen;
        private static readonly Brush _defaultOffColor = Brushes.IndianRed;

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        public ToggleSwitch() { }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ToggleSwitch), new PropertyMetadata(_defaultCornerRadius));

        public Brush ColorON
        {
            get { return (Brush)GetValue(ColorONProperty); }
            set { SetValue(ColorONProperty, value); }
        }

        public static readonly DependencyProperty ColorONProperty =
            DependencyProperty.Register(nameof(ColorON), typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(_defaultOnColor));


        public Brush ColorOFF
        {
            get { return (Brush)GetValue(ColorOFFProperty); }
            set { SetValue(ColorOFFProperty, value); }
        }

        public static readonly DependencyProperty ColorOFFProperty =
            DependencyProperty.Register(nameof(ColorOFF), typeof(Brush), typeof(ToggleSwitch), new PropertyMetadata(_defaultOffColor));

        public string LabelON
        {
            get { return (string)GetValue(LabelONProperty); }
            set { SetValue(LabelONProperty, value); }
        }

        public static readonly DependencyProperty LabelONProperty =
            DependencyProperty.Register(nameof(LabelON), typeof(string), typeof(ToggleSwitch), new PropertyMetadata("ON"));

        public string LabelOFF
        {
            get { return (string)GetValue(LabelOFFProperty); }
            set { SetValue(LabelOFFProperty, value); }
        }

        public static readonly DependencyProperty LabelOFFProperty =
            DependencyProperty.Register(nameof(LabelOFF), typeof(string), typeof(ToggleSwitch), new PropertyMetadata("OFF"));

        public double SwitchWidth
        {
            get { return (double)GetValue(SwitchWidthProperty); }
            set { SetValue(SwitchWidthProperty, value); }
        }

        public static readonly DependencyProperty SwitchWidthProperty =
            DependencyProperty.Register(nameof(SwitchWidth), typeof(double), typeof(ToggleSwitch), new PropertyMetadata(40.0));

        public bool DisplayText
        {
            get { return (bool)GetValue(DisplayTextProperty); }
            set { SetValue(DisplayTextProperty, value); }
        }

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(bool), typeof(ToggleSwitch), new PropertyMetadata(true));
    }
}
