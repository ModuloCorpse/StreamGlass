using StreamGlass.Settings.Settings;
using System.Windows.Controls;

namespace StreamGlass.Settings
{
    public class CheckBoxUserControlLink: UserControlLink
    {
        private readonly CheckBox m_CheckBox;
        public CheckBoxUserControlLink(CheckBox checkBox) => m_CheckBox = checkBox;
        internal override void Load() => m_CheckBox.IsChecked = GetSettings() == "true";
        internal override void Save() => SetSettings((m_CheckBox.IsChecked != null && (bool)m_CheckBox.IsChecked) ? "true" : "false");
    }
}
