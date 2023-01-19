using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Profile;
using System;
using System.Windows;

namespace StreamGlass
{
    public class ProfileMenuItem : MenuItem
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
            CanalManager.Register<string>(StreamGlassCanals.PROFILE_CHANGED_MENU_ITEM, (int _, object? obj) => Dispatcher.Invoke((Delegate)(() => IsChecked = ((string)obj! == m_ProfileID))));
        }

        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            IsChecked = !IsChecked;
            if (IsChecked)
                m_Manager.SetCurrentProfile(m_ProfileID);
            else
                m_Manager.SetCurrentProfile("");
            m_Manager.UpdateStreamInfo();
        }
    }
}
