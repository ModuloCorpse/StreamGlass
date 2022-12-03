using StreamGlass.Settings;
using System.Windows.Controls;

namespace StreamGlass.Twitch
{
    public class SubModeComboBoxUserControlLink : UserControlLink
    {
        private readonly ComboBox m_ComboBox;

        public SubModeComboBoxUserControlLink(ComboBox comboBox) => m_ComboBox = comboBox;

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

        //TODO: To plug
        internal void TranslateComboBox(TranslationManager translation)
        {
            int selectedIndex = m_ComboBox.SelectedIndex;
            m_ComboBox.Items.Clear();
            if (translation.TryGetTranslation("sub_mode_claimed", out var claimedRet))
                m_ComboBox.Items.Add(claimedRet);
            else
                m_ComboBox.Items.Add("Claimed");
            if (translation.TryGetTranslation("sub_mode_all", out var allRet))
                m_ComboBox.Items.Add(allRet);
            else
                m_ComboBox.Items.Add("All");
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
