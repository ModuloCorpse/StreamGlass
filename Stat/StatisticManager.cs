using CorpseLib.Json;
using CorpseLib.ManagedObject;
using CorpseLib.Placeholder;
using System.Collections.Generic;
using System.IO;

namespace StreamGlass.Stat
{
    public class StatisticManager : Manager<StatisticFile>, IContext
    {
        private readonly Dictionary<string, Statistic> m_Statistics = [];

        public StatisticManager() : base("./statistics") { }

        public object? Get(string name) => m_Statistics.TryGetValue(name, out Statistic? value) ? value.Get() : null;
        public T? Get<T>(string name) => m_Statistics.TryGetValue(name, out Statistic? value) ? value.Get<T>() : default;
        public T GetOr<T>(string name, T defaultValue) => m_Statistics.TryGetValue(name, out Statistic? value) ? value.GetOr(defaultValue) : defaultValue;

        private void AddStatistic(Statistic statistic)
        {
            m_Statistics[statistic.Name] = statistic;
            UpdateStatisticFiles(statistic.Name);
        }

        private Statistic GetStatistic(string name)
        {
            if (m_Statistics.TryGetValue(name, out Statistic? statistic))
                return statistic;
            Statistic newStatistic = new(name);
            AddStatistic(newStatistic);
            return newStatistic;
        }

        private void UpdateStatisticFiles(string statistic)
        {
            foreach (StatisticFile file in Objects)
            {
                if (file.CanUpdateFromStatistic(statistic))
                    file.UpdateStatisticFile(this);
            }
        }

        public void CreateStatistic(string name, object value)
        {
            GetStatistic(name).SetValue(value);
            UpdateStatisticFiles(name);
        }

        public void CreateStatistic(string name)
        {
            GetStatistic(name);
            UpdateStatisticFiles(name);
        }

        public void UpdateStatistic(string name, object value)
        {
            GetStatistic(name).SetValue(value);
            UpdateStatisticFiles(name);
        }

        protected override void LoadSettings(JFile obj)
        {
            if (obj.TryGet("stats", out JObject? statsObj))
            {
                foreach (var pair in statsObj!)
                {
                    if (pair.Value is JValue value)
                        AddStatistic(new(pair.Key, value.Value));
                }
            }
        }

        protected override void SaveSettings(ref JFile obj)
        {
            JObject statsObj = [];
            foreach (Statistic statistic in m_Statistics.Values)
            {
                if (statistic.HasValue)
                    statsObj[statistic.Name] = statistic.Value;
            }
            obj["stats"] = statsObj;
        }

        public string? Call(string functionName, string[] args) => null;

        public string? GetVariable(string name)
        {
            if (m_Statistics.TryGetValue(name, out Statistic? statistic))
                return statistic.Get()?.ToString();
            return null;
        }

        protected override StatisticFile? DeserializeObject(JFile obj) => new(obj);

        internal void AddStatisticFile(StatisticFile newFile)
        {
            newFile.UpdateStatisticFile(this);
            AddObject(newFile);
        }

        internal void UpdateStatisticFile(StatisticFile editedFile)
        {
            StatisticFile? oldFile = GetObject(editedFile.ID);
            if (oldFile != null && File.Exists(oldFile.Path))
                File.Delete(oldFile.Path);
            editedFile.UpdateStatisticFile(this);
            SetObject(editedFile);
        }
    }
}
