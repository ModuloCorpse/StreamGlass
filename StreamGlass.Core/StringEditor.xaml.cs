using System.Windows;
using StreamGlass.Core.Controls;

namespace StreamGlass.Core
{
    public partial class StringEditor : Dialog
    {
        private string? m_CreatedString = null;

        public StringEditor(Controls.Window parent) : base(parent)
        {
            InitializeComponent();
            SaveButton.SetTranslationKey(StreamGlassTranslationKeys.SAVE_BUTTON);
        }

        public StringEditor(Controls.Window parent, string str) : this(parent)
        {
            NameTextBox.Text = str;
        }

        internal string? Str => m_CreatedString;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedString = NameTextBox.Text;
            OnOkClick();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedString = null;
            OnCancelClick();
        }
    }
}
