using StreamGlass.Settings.Settings;
using System.Windows.Controls;

namespace StreamGlass.Settings
{
    public class TextBoxUserControlLink: UserControlLink
    {
        private readonly TextBox m_TextBox;
        public TextBoxUserControlLink(TextBox textBox) => m_TextBox = textBox;
        internal override void Load() => m_TextBox.Text = GetSettings();
        internal override void Save() => SetSettings(m_TextBox.Text);
    }
}
