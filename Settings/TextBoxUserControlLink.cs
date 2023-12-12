using StreamGlass.Controls;

namespace StreamGlass.Settings
{
    public class TextBoxUserControlLink(TextBox textBox) : UserControlLink
    {
        private readonly TextBox m_TextBox = textBox;
        protected override void Load() => m_TextBox.Text = GetSettings();
        protected override void Save() => SetSettings(m_TextBox.Text);
    }
}
