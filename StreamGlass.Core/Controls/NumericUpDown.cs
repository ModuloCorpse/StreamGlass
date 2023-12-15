using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace StreamGlass.Core.Controls
{
    public class ValueChangedEventArgs : EventArgs
    {
        public double OldValue;
        public double NewValue;
    }

    //TODO Switch to XAML
    public class NumericUpDown : UserControl
    {
        #region MinValue
        public static readonly DependencyProperty MinValueProperty = Helper.NewProperty("MinValue", double.MinValue, (NumericUpDown i, double v) => i.Property_SetMinValue(v));
        [Description("The minimum value of the numeric up down"), Category("Common Properties")]
        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        internal void Property_SetMinValue(double value)
        {
            if (Value < value)
                Value = value;
        }
        #endregion MinValue

        #region MaxValue
        public static readonly DependencyProperty MaxValueProperty = Helper.NewProperty("MaxValue", double.MaxValue, (NumericUpDown i, double v) => i.Property_SetMaxValue(v));
        [Description("The maximum value of the numeric up down"), Category("Common Properties")]
        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        internal void Property_SetMaxValue(double value)
        {
            if (Value > value)
                Value = value;
        }
        #endregion MaxValue

        #region Value
        public static readonly DependencyProperty ValueProperty = Helper.NewProperty("Value", 0.0, (NumericUpDown i, double v) => i.Property_SetValue(v));
        [Description("The value of the numeric up down"), Category("Common Properties")]
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        internal void Property_SetValue(double value)
        {
            TextBox.Text = value.ToString();
            TextBox.SelectionStart = TextBox.Text.Length;
        }
        #endregion Value

        public event EventHandler<ValueChangedEventArgs>? ValueChanged;

        private readonly DockPanel MainPanel = new() { LastChildFill = true };
        private readonly UniformGrid ButtonsGrid = new() { Columns = 1, Rows = 2 };
        private readonly RepeatButton UpButton = new()
        {
            BrushPaletteKey = "nud_up",
            FontSize = 8,
            FontFamily = new("Marlett"),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        private readonly RepeatButton DownButton = new()
        {
            BrushPaletteKey = "nud_down",
            FontSize = 8,
            FontFamily = new("Marlett"),
            VerticalContentAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center
        };
        private readonly TextBox TextBox = new()
        {
            BrushPaletteKey = "nud_background",
            TextBrushPaletteKey = "nud_text",
            TextAlignment = TextAlignment.Right,
        };

        public NumericUpDown()
        {
            UpButton.Content = "5";
            UpButton.Click += UpButton_Click;
            Grid.SetRow(UpButton, 0);
            BindingOperations.SetBinding(UpButton, WidthProperty, new Binding("ActualHeight"));
            DownButton.Content = "6";
            DownButton.Click += DownButton_Click;
            Grid.SetRow(DownButton, 1);
            BindingOperations.SetBinding(DownButton, WidthProperty, new Binding("ActualHeight"));
            ButtonsGrid.Children.Add(UpButton);
            ButtonsGrid.Children.Add(DownButton);
            System.Windows.Controls.DockPanel.SetDock(ButtonsGrid, Dock.Right);
            TextBox.PreviewKeyDown += TextBox_PreviewKeyDown;
            TextBox.PreviewTextInput += TextBox_PreviewTextInput;
            System.Windows.Controls.DockPanel.SetDock(TextBox, Dock.Left);
            MainPanel.Children.Add(ButtonsGrid);
            MainPanel.Children.Add(TextBox);
            Content = MainPanel;
        }

        public void SetValue(double value)
        {
            double oldValue = Value;
            Value = value;
            ValueChanged?.Invoke(this, new() { OldValue = oldValue, NewValue = Value });
        }

        private void IncreaseValue()
        {
            if (Value <= (MaxValue - 1))
                SetValue(Value + 1);
        }

        private void DecreaseValue()
        {
            if (Value >= (MinValue + 1))
                SetValue(Value - 1);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            IncreaseValue();
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            DecreaseValue();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                IncreaseValue();
            if (e.Key == Key.Down)
                DecreaseValue();
        }

        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
                InsertText((string)e.DataObject.GetData(typeof(string)));
            e.CancelCommand();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InsertText(e.Text);
        }

        private bool InsertText(string str)
        {
            string text = TextBox.Text;
            if (TextBox.SelectionLength > 0)
                text = text.Remove(TextBox.SelectionStart, TextBox.SelectionLength);
            int caretIndex = TextBox.CaretIndex;
            text = text.Insert(caretIndex, str);
            if (!string.IsNullOrWhiteSpace(text) && double.TryParse(text, out var number))
            {
                SetValue(number);
                TextBox.Text = text;
                caretIndex += text.Length;
                TextBox.CaretIndex = caretIndex;
                return true;
            }
            return false;
        }
    }
}
