using CorpseLib.Ini;

namespace StreamGlass.Settings
{
    public abstract class UserControlLink
    {
        private IniSection? m_Settings = null;
        private string m_Setting = string.Empty;

        internal void Init(string setting, IniSection settings)
        {
            m_Setting = setting;
            m_Settings = settings;
            Load();
        }

        protected void SetSettings(string value) => m_Settings!.Set(m_Setting, value);
        protected string GetSettings() => m_Settings!.Get(m_Setting);

        internal void OnSave() => Save();
        internal void OnCancel() => Cancel();

        protected abstract void Load();
        protected abstract void Save();
        protected virtual void Cancel() {}
    }
}
