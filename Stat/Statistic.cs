using CorpseLib;

namespace StreamGlass
{
    public class Statistic(string name)
    {
        private readonly string m_Name = name;
        private object m_Value = new();
        private bool m_HaveValue = false;

        public string Name => m_Name;
        internal object Value => m_Value;
        internal bool HasValue => m_HaveValue;

        public Statistic(string name, object value) : this(name)
        {
            m_Name = name;
            m_Value = value;
            m_HaveValue = true;
        }

        public object? Get() => m_HaveValue ? m_Value : null;
        public T? Get<T>() => m_HaveValue ? Helper.Cast<T>(m_Value) : default;
        public T GetOr<T>(T defaultValue) => m_HaveValue ? Helper.Cast<T>(m_Value) ?? defaultValue : defaultValue;

        public void SetValue(object value)
        {
            m_Value = value;
            m_HaveValue = true;
        }
    }
}
