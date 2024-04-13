﻿using CorpseLib.Json;
using CorpseLib.Placeholder;

namespace StreamGlass.Core.Stat
{
    public abstract class StringSourceAggregator
    {
        private readonly HashSet<string> m_Sources = [];
        private string m_Content = string.Empty;

        internal string[] Sources => [.. m_Sources];
        internal string Content => m_Content;
        internal string AggregatorType => GetAggregatorType();

        protected void Copy(StringSourceAggregator other)
        {
            m_Sources.Clear();
            foreach (var source in other.Sources)
                m_Sources.Add(source);
            m_Content = other.Content;
        }

        public void AddSource(string source) => m_Sources.Add(source);
        public void RemoveSource(string source) => m_Sources.Remove(source);
        public void SetContent(string content) => m_Content = content;

        internal bool CanAggregateFromSource(string updatedSource) => m_Sources.Contains(updatedSource);

        internal void Aggregate()
        {
            if (!string.IsNullOrEmpty(m_Content))
            {
                string text = Converter.Convert(m_Content, new StreamGlassContext());
                OnAggregate(text);
            }
        }

        internal void Save(JsonObject json)
        {
            OnSave(json);
            json["sources"] = Sources;
            if (!string.IsNullOrEmpty(m_Content))
                json["content"] = m_Content;
        }

        internal void Load(JsonObject json)
        {
            List<string> sources = json.GetList<string>("sources");
            foreach (string source in sources)
                AddSource(source);
            if (json.TryGet("content", out string? content) && !string.IsNullOrEmpty(content))
                SetContent(content);
            OnLoad(json);
        }

        protected abstract void OnSave(JsonObject json);
        protected abstract void OnLoad(JsonObject json);
        protected abstract string GetAggregatorType();
        protected abstract void OnAggregate(string text);
    }
}
