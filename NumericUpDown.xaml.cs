using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamGlass
{
    public class ValueChangedEventArgs : EventArgs
    {
        public double OldValue;
        public double NewValue;
    }

    public partial class NumericUpDown : UserControl
    {
        #region MinValue
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register("MinValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0));
        [Description("The maximum value of the numeric up down"), Category("Common Properties")]
        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        #endregion MinValue

        #region MaxValue
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0));
        [Description("The minimum value of the numeric up down"), Category("Common Properties")]
        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        #endregion MaxValue

        #region Value
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(0.0));
        [Description("The value of the numeric up down"), Category("Common Properties")]
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        #endregion Value

        public event EventHandler<ValueChangedEventArgs>? ValueChanged;

        public NumericUpDown()
        {
            InitializeComponent();
        }

        public void QuietSetValue(double value)
        {
            if (value > MaxValue)
                value = MaxValue;
            if (value < MinValue)
                value = MinValue;
            Value = value;
            NUDTextBox.Text = Value.ToString();
        }

        public void SetValue(double value)
        {
            double oldValue = Value;
            QuietSetValue(value);
            ValueChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = Value });
        }

        private void NUDButtonUP_Click(object sender, RoutedEventArgs e)
        {
            if (Value < MaxValue)
                NUDTextBox.Text = (Value + 1).ToString();
        }

        private void NUDButtonDown_Click(object sender, RoutedEventArgs e)
        {
            if (Value > MinValue)
                NUDTextBox.Text = (Value - 1).ToString();
        }

        private void NUDTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                NUDButtonUP.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button)?.GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonUP, new object[] { true });
            }
            if (e.Key == Key.Down)
            {
                NUDButtonDown.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                typeof(Button)?.GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonDown, new object[] { true });
            }
        }

        private void NUDTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                typeof(Button)?.GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonUP, new object[] { false });
            if (e.Key == Key.Down)
                typeof(Button)?.GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(NUDButtonDown, new object[] { false });
        }

        private void NUDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NUDTextBox.Text) && double.TryParse(NUDTextBox.Text, out var number))
            {
                SetValue(number);
                NUDTextBox.SelectionStart = NUDTextBox.Text.Length;
            }
        }

        private static bool IsTextAllowed(string text)
        {
            return double.TryParse(text, out var _);
        }

        private void NUDTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)) ||
                !IsTextAllowed((string)e.DataObject.GetData(typeof(string))))
                e.CancelCommand();
        }

        private void NUDTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
