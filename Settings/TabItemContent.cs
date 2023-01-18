using StreamFeedstock;
using StreamFeedstock.Controls;
using System.Collections.Generic;

namespace StreamGlass.Settings
{
    public abstract class TabItemContent : UserControl
    {
        protected Dialog? m_SettingsDialog = null;
        private readonly Data m_Settings;
        private readonly List<UserControlLink> m_Links = new();
        private readonly string m_HeaderSource;
        private readonly string m_SettingCategory;

        protected TabItemContent(string headerSource, string settingCategory, Data settings)
        {
            m_HeaderSource = headerSource;
            m_SettingCategory = settingCategory;
            m_Settings = settings;
        }

        internal void SetSettingDialog(Dialog dialog)
        {
            m_SettingsDialog = dialog;
            OnInit();
        }

        protected void AddControlLink(string setting, UserControlLink link)
        {
            link.Init(m_SettingCategory, setting, m_Settings);
            m_Links.Add(link);
        }

        protected string GetSetting(string setting) => m_Settings.Get(m_SettingCategory, setting);
        protected void SetSetting(string setting, string value) => m_Settings.Set(m_SettingCategory, setting, value);

        internal string GetHeaderSource() => m_HeaderSource;

        internal void SaveTabItem()
        {
            foreach (UserControlLink link in m_Links)
                link.OnSave();
            OnSave();
        }

        internal void CancelTabItem()
        {
            foreach (UserControlLink link in m_Links)
                link.OnCancel();
            OnCancel();
        }

        internal void UpdateTabItemColorPalette(BrushPaletteManager manager, TranslationManager translation) => Update(manager, translation);

        protected virtual void OnInit() {}

        protected virtual void OnSave() {}

        protected virtual void OnCancel() {}
    }
}
