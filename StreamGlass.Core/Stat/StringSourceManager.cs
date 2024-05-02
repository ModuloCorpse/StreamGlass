using CorpseLib.DataNotation;
using CorpseLib.Json;
using CorpseLib.Placeholder;

namespace StreamGlass.Core.Stat
{
    public class StringSourceManager : IContext
    {
        public delegate StringSourceAggregator AggregatorMaker();

        private readonly Dictionary<string, StringSource> m_StringSources = [];
        private readonly Dictionary<string, AggregatorMaker> m_AggregatorTypes = [];
        private readonly Dictionary<string, List<StringSourceAggregator>> m_Aggregators = [];

        public string[] AggregatorTypes => [.. m_AggregatorTypes.Keys];

        public StringSourceManager()
        {
            RegisterAggregator(() => new StringSourceFile());
        }

        public void RegisterAggregator(AggregatorMaker aggregatorMaker)
        {
            string aggregatorType = aggregatorMaker().AggregatorType;
            if (m_AggregatorTypes.ContainsKey(aggregatorType))
                return;
            m_AggregatorTypes[aggregatorType] = aggregatorMaker;
            m_Aggregators[aggregatorType] = [];
        }

        public StringSourceAggregator[] GetAggregators(string aggregatorType)
        {
            if (m_Aggregators.TryGetValue(aggregatorType, out var aggregator))
                return [..aggregator];
            return [];
        }

        public void NewAggregator(string type, DataObject data, bool unsavable)
        {
            if (m_AggregatorTypes.TryGetValue(type, out AggregatorMaker? maker))
            {
                StringSourceAggregator aggregator = maker();
                aggregator.Load(data);
                if (unsavable)
                    aggregator.MakeUnsavable();
                aggregator.Aggregate();
                if (m_Aggregators.TryGetValue(aggregator.AggregatorType, out var aggregatorList))
                    aggregatorList.Add(aggregator);
            }
        }

        public void Add(StringSourceAggregator aggregator)
        {
            if (m_Aggregators.TryGetValue(aggregator.AggregatorType, out var aggregators))
            {
                aggregator.Aggregate();
                aggregators.Add(aggregator);
            }
        }

        public void Remove(StringSourceAggregator aggregator)
        {
            if (m_Aggregators.TryGetValue(aggregator.AggregatorType, out var aggregators))
                aggregators.Remove(aggregator);
        }

        public void Load()
        {
            DataObject json = JsonParser.LoadFromFile("settings.json");
            if (json.TryGet("sources", out DataObject? statsObj))
            {
                foreach (var pair in statsObj!)
                {
                    if (pair.Value is DataValue value && value.Value is string str)
                        AddStringSource(new(pair.Key, str));
                }
            }
            List<DataObject> aggregators = json.GetList<DataObject>("aggregators");
            foreach (DataObject aggregatorData in aggregators)
            {
                if (aggregatorData.TryGet("type", out string? type) && type != null)
                    NewAggregator(type, aggregatorData, false);
            }
        }

        public void SaveTo(DataObject json)
        {
            DataObject statsObj = [];
            foreach (StringSource stringSource in m_StringSources.Values)
            {
                if (stringSource.HasValue)
                    statsObj[stringSource.Name] = stringSource.Value;
            }
            json["sources"] = statsObj;
            List<DataObject> aggregators = [];
            foreach (var pair in m_Aggregators)
            {
                foreach (StringSourceAggregator aggregator in pair.Value)
                {
                    if (aggregator.CanSave)
                    {
                        DataObject aggregatorObj = new() { { "type", pair.Key } };
                        aggregator.Save(aggregatorObj);
                        aggregators.Add(aggregatorObj);
                    }
                }
            }
            json["aggregators"] = aggregators;
        }

        public string? Get(string name) => m_StringSources.TryGetValue(name, out StringSource? value) ? value.Get() : null;
        public string GetOr(string name, string defaultValue) => m_StringSources.TryGetValue(name, out StringSource? value) ? value.GetOr(defaultValue) : defaultValue;

        private void AddStringSource(StringSource source)
        {
            m_StringSources[source.Name] = source;
            OnStringSourceUpdated(source.Name);
        }

        private StringSource GetStringSource(string name)
        {
            if (m_StringSources.TryGetValue(name, out StringSource? source))
                return source;
            StringSource newSource = new(name);
            AddStringSource(newSource);
            return newSource;
        }

        private void OnStringSourceUpdated(string source)
        {
            foreach (var aggregators in m_Aggregators.Values)
            {
                foreach (StringSourceAggregator file in aggregators)
                {
                    if (file.CanAggregateFromSource(source))
                        file.Aggregate();
                }
            }
        }

        public void CreateStringSource(string name, string value)
        {
            GetStringSource(name).SetValue(value);
            OnStringSourceUpdated(name);
        }

        public void CreateStringSource(string name)
        {
            GetStringSource(name);
            OnStringSourceUpdated(name);
        }

        public void UpdateStringSource(string name, string value)
        {
            GetStringSource(name).SetValue(value);
            OnStringSourceUpdated(name);
        }

        public string? Call(string functionName, string[] args, Cache cache) => null;

        public string? GetVariable(string name)
        {
            if (m_StringSources.TryGetValue(name, out StringSource? stringSource))
                return stringSource.Get();
            return null;
        }
    }
}
