﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace StreamGlass.Core.Controls
{
    public class PasswordBox : System.Windows.Controls.TextBox, IUIElement
    {
        #region HideChars
        public static readonly DependencyProperty HideCharsProperty = Helper.NewProperty("HideChars", true, (PasswordBox i, bool _) => i.UpdateTextValue());
        public bool HideChars
        {
            get => (bool)GetValue(HideCharsProperty);
            set => SetValue(HideCharsProperty, value);
        }
        #endregion HideChars

        #region Password
        public static readonly DependencyProperty PasswordProperty = Helper.NewProperty("Password", string.Empty, (PasswordBox i, string _) => i.UpdateTextValue());
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }
        #endregion Password

        #region PasswordChar
        public static readonly DependencyProperty PasswordCharProperty = Helper.NewProperty("PasswordChar", '*', (PasswordBox i, char _) => i.UpdateTextValue());
        public char PasswordChar
        {
            get => (char)GetValue(PasswordCharProperty);
            set => SetValue(PasswordCharProperty, value);
        }
        #endregion PasswordChar

        #region BrushPaletteKey
        public static readonly DependencyProperty BrushPaletteKeyProperty = Helper.NewProperty<PasswordBox, string>("BrushPaletteKey", "background_2");
        [Description("The brush palette key of the password box's background"), Category("Common Properties")]
        public string BrushPaletteKey
        {
            get => (string)GetValue(BrushPaletteKeyProperty);
            set => SetValue(BrushPaletteKeyProperty, value);
        }
        #endregion BrushPaletteKey

        #region TextBrushPaletteKey
        public static readonly DependencyProperty TextBrushPaletteKeyProperty = Helper.NewProperty<PasswordBox, string>("TextBrushPaletteKey", "text");
        [Description("The brush palette key of the password box's text"), Category("Common Properties")]
        public string TextBrushPaletteKey
        {
            get => (string)GetValue(TextBrushPaletteKeyProperty);
            set => SetValue(TextBrushPaletteKeyProperty, value);
        }
        #endregion TextBrushPaletteKey

        public PasswordBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            CommandManager.AddPreviewExecutedHandler(this, PreviewExecutedHandler);
        }

        private static void PreviewExecutedHandler(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            if (executedRoutedEventArgs.Command == ApplicationCommands.Copy ||
                executedRoutedEventArgs.Command == ApplicationCommands.Cut ||
                executedRoutedEventArgs.Command == ApplicationCommands.Paste)
                executedRoutedEventArgs.Handled = true;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            InsertText(textCompositionEventArgs.Text);
            textCompositionEventArgs.Handled = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            Key pressedKey = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
            switch (pressedKey)
            {
                case Key.Space:
                    {
                        InsertText(" ");
                        keyEventArgs.Handled = true;
                        break;
                    }
                case Key.Back:
                    {
                        int caretIndex = CaretIndex;
                        if (SelectionLength > 0)
                            Password = Password.Remove(SelectionStart, SelectionLength);
                        else if (CaretIndex > 0)
                        {
                            if (CaretIndex > 0 && CaretIndex < Text.Length)
                                caretIndex -= 1;
                            Password = Password.Remove(CaretIndex - 1, 1);
                        }
                        CaretIndex = caretIndex;
                        keyEventArgs.Handled = true;
                        break;
                    }
                case Key.Delete:
                    {
                        if (SelectionLength > 0)
                            Password = Password.Remove(SelectionStart, SelectionLength);
                        else if (CaretIndex < Text.Length)
                            Password = Password.Remove(CaretIndex, 1);
                        keyEventArgs.Handled = true;
                        break;
                    }
            }
        }

        private void InsertText(string text)
        {
            int caretIndex = CaretIndex;
            if (SelectionLength > 0)
                Password = Password.Remove(SelectionStart, SelectionLength);
            Password = Password.Insert(caretIndex, text);
            caretIndex += text.Length;
            CaretIndex = caretIndex;
        }

        private void UpdateTextValue()
        {
            Text = (HideChars) ? new string(PasswordChar, Password.Length) : Password;
        }

        public void Update(BrushPaletteManager palette)
        {
            if (palette.TryGetColor(BrushPaletteKey, out var background))
                Background = background;
            if (palette.TryGetColor(TextBrushPaletteKey, out var foreground))
                Foreground = foreground;
        }
    }
}
