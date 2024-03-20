using StreamGlass.Core.Settings;
using System.Windows.Controls;

namespace StreamGlass.Twitch
{
    public class SubModeComboBoxUserControlLink(ComboBox comboBox) : UserControlLink
    {
        private readonly ComboBox m_ComboBox = comboBox;

        protected override void Load()
        {
            m_ComboBox.Items.Clear();
            m_ComboBox.Items.Add("Claimed");
            m_ComboBox.Items.Add("All");
            if (GetSettings() == "claimed")
                m_ComboBox.SelectedIndex = 0;
            else
                m_ComboBox.SelectedIndex = 1;
        }

        internal void TranslateComboBox()
        {
            int selectedIndex = m_ComboBox.SelectedIndex;
            m_ComboBox.Items.Clear();
            m_ComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_CLAIMED.ToString());
            m_ComboBox.Items.Add(TwitchPlugin.TranslationKeys.SETTINGS_TWITCH_SUB_MODE_ALL.ToString());
            m_ComboBox.SelectedIndex = selectedIndex;
        }

        protected override void Save()
        {
            if (m_ComboBox.SelectedIndex == 0)
                SetSettings("claimed");
            else
                SetSettings("all");
        }
    }
}
