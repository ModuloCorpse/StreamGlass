using CorpseLib.Json;
using CorpseLib.Placeholder;
using System.Collections.Generic;
using System.Linq;

namespace StreamGlass.Stat
{
    public class StatisticManager : IContext
    {
        private readonly Dictionary<string, Statistic> m_Statistics = new();

        public object? Get(string name) => m_Statistics.ContainsKey(name) ? m_Statistics[name].Get() : null;
        public T? Get<T>(string name) => m_Statistics.ContainsKey(name) ? m_Statistics[name].Get<T>() : default;
        public T GetOr<T>(string name, T defaultValue) => m_Statistics.ContainsKey(name) ? m_Statistics[name].GetOr(defaultValue) : defaultValue;

        private void AddStatistic(Statistic statistic)
        {
            statistic.SetManager(this);
            m_Statistics[statistic.Name] = statistic;
        }

        private Statistic GetStatistic(string name)
        {
            if (m_Statistics.TryGetValue(name, out Statistic? statistic))
                return statistic;
            Statistic newStatistic = new(name);
            AddStatistic(newStatistic);
            return newStatistic;
        }

        public void SetStatisticFile(string name, string filePath) => GetStatistic(name).SetFilePath(filePath);
        public void SetStatisticFileContent(string name, string fileContent) => GetStatistic(name).SetFileContent(fileContent);

        public void CreateStatistic(string name, object value) => GetStatistic(name).SetValue(value);
        public void CreateStatistic(string name) => GetStatistic(name);

        public void UpdateStatistic(string name, object value) => GetStatistic(name).SetValue(value);

        public void Save() => new JFile() { ["stats"] = m_Statistics.Values.ToList() }.WriteToFile("statistics.json");

        public void Load()
        {
            JFile file = JFile.LoadFromFile("statistics.json");
            List<Statistic> list = file.GetList<Statistic>("stats");
            foreach (Statistic statistic in list)
                AddStatistic(statistic);
            foreach (Statistic statistic in m_Statistics.Values)
                statistic.UpdateStatisticFile();
        }

        public string? Call(string functionName, string[] args) => null;

        public string? GetVariable(string name)
        {
            if (m_Statistics.TryGetValue(name, out Statistic? statistic))
                return statistic.Get()?.ToString();
            return null;
        }
    }
}
