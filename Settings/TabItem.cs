using StreamGlass.Settings.Settings;
using System.Collections.Generic;
using System.Windows.Controls;

namespace StreamGlass.Settings
{
    public abstract class TabItem : UserControl
    {
        private readonly string m_HeaderSource;
        private readonly string m_SettingCategory;
        private readonly Data m_Settings;
        private readonly List<UserControlLink> m_Links = new();

        protected TabItem(string headerSource, string settingCategory, Data settings)
        {
            m_HeaderSource = headerSource;
            m_SettingCategory = settingCategory;
            m_Settings = settings;
        }

        protected void AddControlLink(string setting, UserControlLink link)
        {
            link.Init(m_SettingCategory, setting, m_Settings);
            m_Links.Add(link);
        }

        internal string GetHeaderSource() => m_HeaderSource;

        internal void SaveTabItem()
        {
            foreach (UserControlLink link in m_Links)
                link.Save();
            OnSave();
        }

        protected virtual void OnSave() {}
    }
}
