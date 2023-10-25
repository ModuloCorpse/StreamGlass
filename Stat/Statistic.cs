using CorpseLib;
using CorpseLib.Json;
using CorpseLib.Placeholder;
using StreamGlass.Stat;
using System.IO;

namespace StreamGlass
{
    public class Statistic
    {
        private readonly string m_Name;
        private object m_Value = new();
        private bool m_HaveValue = false;

        public string Name => m_Name;
        internal object Value => m_Value;
        internal bool HasValue => m_HaveValue;

        public Statistic(string name) => m_Name = name;

        public Statistic(string name, object value) : this(name)
        {
            m_Name = name;
            m_Value = value;
            m_HaveValue = true;
        }

        public object? Get() => m_HaveValue ? m_Value : null;
        public T? Get<T>() => m_HaveValue ? (T)m_Value : default;
        public T GetOr<T>(T defaultValue) => m_HaveValue ? (T)m_Value : defaultValue;

        public void SetValue(object value)
        {
            m_Value = value;
            m_HaveValue = true;
        }
    }
}
