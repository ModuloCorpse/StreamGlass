using CorpseLib.Json;
using CorpseLib.ManagedObject;
using CorpseLib.Placeholder;
using System.IO;

namespace StreamGlass.Core.Stat
{
    public class StringSourceManager : Manager<StringSourceFile>, IContext
    {
        private readonly Dictionary<string, StringSource> m_StringSources = [];

        public StringSourceManager() : base("./strsrcs") { }

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
            //TODO Rework how string source update listeners
            foreach (StringSourceFile file in Objects)
            {
                if (file.CanUpdateFromSource(source))
                    file.UpdateStringSourceFile();
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

        protected override void LoadSettings(JsonObject obj)
        {
            if (obj.TryGet("sources", out JsonObject? statsObj))
            {
                foreach (var pair in statsObj!)
                {
                    if (pair.Value is JsonValue value && value.Value is string str)
                        AddStringSource(new(pair.Key, str));
                }
            }
        }

        protected override void SaveSettings(ref JsonObject obj)
        {
            JsonObject statsObj = [];
            foreach (StringSource stringSource in m_StringSources.Values)
            {
                if (stringSource.HasValue)
                    statsObj[stringSource.Name] = stringSource.Value;
            }
            obj["sources"] = statsObj;
        }

        public string? Call(string functionName, string[] args, Cache cache) => null;

        public string? GetVariable(string name)
        {
            if (m_StringSources.TryGetValue(name, out StringSource? stringSource))
                return stringSource.Get();
            return null;
        }

        protected override StringSourceFile? DeserializeObject(JsonObject obj) => new(obj);

        internal void AddStringSourceFile(StringSourceFile newFile)
        {
            newFile.UpdateStringSourceFile();
            AddObject(newFile);
        }

        internal void UpdateStringSourceFile(StringSourceFile editedFile)
        {
            StringSourceFile? oldFile = GetObject(editedFile.ID);
            if (oldFile != null && File.Exists(oldFile.Path))
                File.Delete(oldFile.Path);
            editedFile.UpdateStringSourceFile();
            SetObject(editedFile);
        }
    }
}
