using CorpseLib;
using CorpseLib.Json;
using CorpseLib.Placeholder;
using StreamGlass.Stat;
using System.IO;

namespace StreamGlass
{
    public class Statistic
    {
        [DefaultJSerializer]
        public class StatisticJSerializer : AJSerializer<Statistic>
        {
            protected override OperationResult<Statistic> Deserialize(JObject reader)
            {
                if (reader.TryGet("name", out string? name))
                {
                    Statistic stat = new(name!);
                    if (reader.TryGet("value", out JValue? value) && value != null)
                        stat.SetValue(value.Value);
                    if (reader.TryGet("path", out string? path) && !string.IsNullOrEmpty(path))
                        stat.SetFilePath(path);
                    if (reader.TryGet("content", out string? content) && !string.IsNullOrEmpty(path))
                        stat.SetFileContent(content!);
                    return new(stat);
                }
                return new("Invalid statistic", "Statistic has no name");
            }

            protected override void Serialize(Statistic obj, JObject writer)
            {
                writer.Set("name", obj.Name);
                if (obj.HasValue)
                    writer.Set("value", obj.Value);
                if (!string.IsNullOrEmpty(obj.FilePath))
                    writer.Set("path", obj.FilePath);
                writer.Set("content", obj.FileContent);
            }
        }

        private StatisticManager? m_Manager = null;
        private readonly string m_Name;
        private object m_Value = new();
        private string m_FilePath = string.Empty;
        private string m_FileContent;
        private bool m_HaveValue = false;

        public string Name => m_Name;
        internal object Value => m_Value;
        internal string FilePath => m_FilePath;
        internal string FileContent => m_FileContent;
        internal bool HasValue => m_HaveValue;

        public Statistic(string name)
        {
            m_Name = name;
            m_FileContent = "${" + name + "}";
        }

        internal void SetManager(StatisticManager manager) => m_Manager = manager;

        public object? Get() => m_HaveValue ? m_Value : null;
        public T? Get<T>() => m_HaveValue ? (T)m_Value : default;
        public T GetOr<T>(T defaultValue) => m_HaveValue ? (T)m_Value : defaultValue;

        public void UpdateStatisticFile()
        {
            if (m_HaveValue && !string.IsNullOrEmpty(m_FilePath))
            {
                if (m_Manager != null)
                    File.WriteAllText(m_FilePath, Converter.Convert(m_FileContent, m_Manager));
                else
                    File.WriteAllText(m_FilePath, m_Value.ToString());
            }
        }

        public void SetFilePath(string filePath)
        {
            m_FilePath = filePath;
            UpdateStatisticFile();
        }

        public void SetFileContent(string fileContent)
        {
            m_FileContent = fileContent;
            UpdateStatisticFile();
        }

        public void SetValue(object value)
        {
            m_Value = value;
            m_HaveValue = true;
            UpdateStatisticFile();
        }
    }
}
