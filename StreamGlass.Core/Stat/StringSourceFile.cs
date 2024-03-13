using CorpseLib.Json;
using CorpseLib.Placeholder;
using System.IO;
using System.Text;
using CorpseLib.ManagedObject;

namespace StreamGlass.Core.Stat
{
    public class StringSourceFile : Object<StringSourceFile>
    {
        private readonly HashSet<string> m_Sources = [];
        private string m_Content = string.Empty;

        internal StringSourceFile(string id, string path) : base(id, path) { }
        internal StringSourceFile(JsonObject json) : base(json) { }

        internal List<string> Sources => [.. m_Sources];
        internal string Path => Name;
        internal string Content => m_Content;

        public void AddSource(string source) => m_Sources.Add(source);
        public void RemoveSource(string source) => m_Sources.Remove(source);
        public void SetContent(string content) => m_Content = content;

        public bool CanUpdateFromSource(string updatedSource) => m_Sources.Contains(updatedSource);

        public void UpdateStringSourceFile()
        {
            if (!string.IsNullOrEmpty(m_Content))
                File.WriteAllText(Path, Converter.Convert(m_Content, new StreamGlassContext()));
        }

        protected override void Save(ref JsonObject json)
        {
            json.Add("sources", m_Sources.ToList());
            if (!string.IsNullOrEmpty(m_Content))
                json.Add("content", m_Content);
        }

        protected override void Load(JsonObject json)
        {
            List<string> sources = json.GetList<string>("sources");
            foreach (string source in sources)
                AddSource(source);
            if (json.TryGet("content", out string? content) && !string.IsNullOrEmpty(content))
                SetContent(content);
        }
    }
}
