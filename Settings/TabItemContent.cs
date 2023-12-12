using CorpseLib.Ini;
using StreamGlass.Controls;
using System.Collections.Generic;

namespace StreamGlass.Settings
{
    public abstract class TabItemContent : UserControl
    {
        protected Dialog? m_SettingsDialog = null;
        private readonly IniSection m_Settings;
        private readonly List<UserControlLink> m_Links = [];
        private readonly string m_HeaderSource;

        protected TabItemContent(string headerSource, IniSection settings)
        {
            m_HeaderSource = headerSource;
            m_Settings = settings;
        }

        internal void SetSettingDialog(Dialog dialog)
        {
            m_SettingsDialog = dialog;
            OnInit();
        }

        protected void AddControlLink(string setting, UserControlLink link)
        {
            link.Init(setting, m_Settings);
            m_Links.Add(link);
        }

        protected string GetSetting(string setting) => m_Settings.Get(setting);
        protected void SetSetting(string setting, string value) => m_Settings.Set(setting, value);

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

        internal void UpdateTabItemColorPalette(BrushPaletteManager manager) => Update(manager);

        protected virtual void OnInit() {}

        protected virtual void OnSave() {}

        protected virtual void OnCancel() {}
    }
}
