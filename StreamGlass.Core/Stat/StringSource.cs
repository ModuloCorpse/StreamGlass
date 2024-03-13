namespace StreamGlass.Core.Stat
{
    public class StringSource(string name)
    {
        private readonly string m_Name = name;
        private string m_Value = string.Empty;
        private bool m_HaveValue = false;

        public string Name => m_Name;
        internal string Value => m_Value;
        internal bool HasValue => m_HaveValue;

        public StringSource(string name, string value) : this(name)
        {
            m_Name = name;
            m_Value = value;
            m_HaveValue = true;
        }

        public string? Get() => m_HaveValue ? m_Value : null;
        public string GetOr(string defaultValue) => m_HaveValue ? m_Value : defaultValue;

        public void SetValue(string value)
        {
            m_Value = value;
            m_HaveValue = true;
        }
    }
}
