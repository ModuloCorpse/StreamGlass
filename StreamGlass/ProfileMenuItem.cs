using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;
using System.Windows;

namespace StreamGlass
{
    public class ProfileMenuItem : MenuItem, RadioGroup<string>.IItem
    {
        private readonly ProfileManager m_Manager;
        private readonly string m_ProfileID;

        public ProfileMenuItem(ProfileManager manager, string profileID)
        {
            BrushPaletteKey = "top_bar_background";
            m_Manager = manager;
            m_ProfileID = profileID;
            Click += ProfileMenuItem_Click;
            if (m_Manager.CurrentObjectID == m_ProfileID)
                IsChecked = true;
        }

        public string GetID() => m_ProfileID;

        public void OnSelected() => Dispatcher.Invoke(() => IsChecked = true);

        public void OnUnselected() => Dispatcher.Invoke(() => IsChecked = false);

        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsChecked = !IsChecked;
            if (IsChecked)
                m_Manager.SetCurrentProfile(m_ProfileID);
            else
                m_Manager.SetCurrentProfile(string.Empty);
            m_Manager.UpdateStreamInfo();
        }
    }
}
