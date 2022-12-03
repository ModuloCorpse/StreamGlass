using System.Collections.Generic;
using System.IO;

namespace StreamGlass.Settings
{
    public class Data
    {
        private Dictionary<string, Dictionary<string, string>> m_Settings = new();

        public void Load()
        {
            if (File.Exists("settings.json"))
            {
                Json response = Json.LoadFromFile("settings.json");
                foreach (var item in response.ToDictionary<Json>())
                {
                    string key = item.Key;
                    Json value = item.Value;
                    m_Settings[key] = value.ToDictionary<string>();
                }
            }
        }

        public string Get(string section, string name)
        {
            if (m_Settings.TryGetValue(section, out var settings))
            {
                if (settings.TryGetValue(name, out var value))
                    return value;
            }
            return "";
        }

        public void Set(string section, string name, string value)
        {
            if (!m_Settings.ContainsKey(section))
                m_Settings.Add(section, new());
            m_Settings[section][name] = value;
        }

        public void Create(string section, string name, string value)
        {
            if (!m_Settings.ContainsKey(section))
            {
                m_Settings.Add(section, new());
                m_Settings[section][name] = value;
            }
        }

        public void Save()
        {
            Json json = new();
            foreach (var setting in m_Settings)
            {
                Json section = new();
                section.Set(setting.Value);
                json.Set(setting.Key, section);
            }
            json.WriteToFile("settings.json");
        }
    }
}
