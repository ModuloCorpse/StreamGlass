using System.Windows.Controls;

namespace StreamGlass.Core.Settings
{
    public class CheckBoxUserControlLink(CheckBox checkBox) : UserControlLink
    {
        private readonly CheckBox m_CheckBox = checkBox;
        protected override void Load() => m_CheckBox.IsChecked = GetSettings() == "true";
        protected override void Save() => SetSettings((m_CheckBox.IsChecked != null && (bool)m_CheckBox.IsChecked) ? "true" : "false");
    }
}
