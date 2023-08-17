using System.Windows;
using StreamGlass.Controls;

namespace StreamGlass.Profile
{
    public partial class StringEditor : Dialog
    {
        private string? m_CreatedString = null;

        public StringEditor(StreamGlass.Controls.Window parent): base(parent)
        {
            InitializeComponent();
        }

        public StringEditor(StreamGlass.Controls.Window parent, string str): this(parent)
        {
            NameTextBox.Text = str;
        }

        internal string? Str => m_CreatedString;

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedString = NameTextBox.Text;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            m_CreatedString = null;
            Close();
        }
    }
}
