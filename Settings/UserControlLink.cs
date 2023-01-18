namespace StreamGlass.Settings
{
    public abstract class UserControlLink
    {
        private Data? m_Settings = null;
        private string m_Category = "";
        private string m_Setting = "";

        internal void Init(string category, string setting, Data settings)
        {
            m_Category = category;
            m_Setting = setting;
            m_Settings = settings;
            Load();
        }

        protected void SetSettings(string value) => m_Settings!.Set(m_Category, m_Setting, value);
        protected string GetSettings() => m_Settings!.Get(m_Category, m_Setting);

        internal void OnSave() => Save();
        internal void OnCancel() => Cancel();

        protected abstract void Load();
        protected abstract void Save();
        protected virtual void Cancel() {}
    }
}
