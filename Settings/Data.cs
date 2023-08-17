using CorpseLib.Json;
using StreamGlass;
using System.Collections.Generic;
using System.IO;

namespace StreamGlass.Settings
{
    public class Data
    {
        private readonly Dictionary<string, Dictionary<string, string>> m_Settings = new();

        public void Load()
        {
            if (File.Exists("settings.json"))
            {
                JFile response = JFile.LoadFromFile("settings.json");
                foreach (var item in response)
                {
                    string key = item.Key;
                    JObject value = item.Value.Cast<JObject>()!;
                    Dictionary<string, string> setting = new();
                    foreach (var item2 in value)
                        setting[item2.Key] = item2.Value.Cast<string>()!;
                    m_Settings[key] = setting;
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
                m_Settings.Add(section, new());
            if (!m_Settings[section].ContainsKey(name))
                m_Settings[section][name] = value;
        }

        public void Save()
        {
            JFile json = new();
            foreach (var setting in m_Settings)
            {
                JObject section = new();
                foreach(var item in setting.Value)
                    section.Set(item.Key, item.Value);
                json.Set(setting.Key, section);
            }
            json.WriteToFile("settings.json");
        }
    }
}
