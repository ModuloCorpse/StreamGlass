using CorpseLib.Json;
using CorpseLib.Placeholder;
using System.IO;
using System.Text;
using CorpseLib.ManagedObject;

namespace StreamGlass.Core.Stat
{
    public class StatisticFile : Object<StatisticFile>
    {
        private readonly HashSet<string> m_Statistics = [];
        private string m_Content = string.Empty;

        internal StatisticFile(string id, string path) : base(id, path) { }
        internal StatisticFile(JObject json) : base(json) { }

        internal List<string> Statistics => [.. m_Statistics];
        internal string Path => Name;
        internal string Content => m_Content;

        public void AddStatistic(string statistic) => m_Statistics.Add(statistic);
        public void RemoveStatistic(string statistic) => m_Statistics.Remove(statistic);
        public void SetContent(string content) => m_Content = content;

        public bool CanUpdateFromStatistic(string updatedStatistic) => m_Statistics.Contains(updatedStatistic);

        public void UpdateStatisticFile(StatisticManager manager)
        {
            if (!string.IsNullOrEmpty(m_Content))
                File.WriteAllText(Path, Converter.Convert(m_Content, new StreamGlassContext()));
            else
            {
                StringBuilder builder = new();
                foreach (string statistic in m_Statistics)
                {
                    string? value = manager.Get(statistic)?.ToString();
                    if (value != null)
                        builder.AppendLine(value);
                }
                File.WriteAllText(Path, builder.ToString());
            }
        }

        protected override void Save(ref JObject json)
        {
            json.Add("stats", m_Statistics.ToList());
            if (!string.IsNullOrEmpty(m_Content))
                json.Add("content", m_Content);
        }

        protected override void Load(JObject json)
        {
            List<string> statistics = json.GetList<string>("stats");
            foreach (string statistic in statistics)
                AddStatistic(statistic);
            if (json.TryGet("content", out string? content) && !string.IsNullOrEmpty(content))
                SetContent(content!);
        }
    }
}
