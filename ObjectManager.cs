using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace StreamGlass
{
    public abstract class ObjectManager<T> where T : ManagedObject<T>
    {
        private readonly string m_FilePath;
        private readonly Dictionary<string, T> m_Objects = new();
        private T? m_CurrentObject = null;

        protected ObjectManager(string filePath) => m_FilePath = filePath;

        protected void AddObject(T obj)
        {
            m_Objects[obj.ID] = obj;
        }

        protected bool SetCurrentObject(string id)
        {
            if (m_Objects.TryGetValue(id, out T? obj))
            {
                m_CurrentObject = obj;
                return true;
            }
            return false;
        }

        protected T? CurrentObject => m_CurrentObject;
        public List<T> Objects => m_Objects.Values.ToList();
        public string CurrentObjectID => (m_CurrentObject != null) ? m_CurrentObject.ID : "";

        public JObject Serialize()
        {
            JObject json = new();
            JArray objects = new();
            foreach (var obj in m_Objects.Values)
            {
                if (obj.IsSerializable())
                    objects.Add(obj.Serialize());
            }
            if (m_CurrentObject != null)
                json["current"] = m_CurrentObject.ID;
            json["objects"] = objects;
            return json;
        }

        public void Deserialize(JObject json)
        {
            JArray? profiles = (JArray?)json["objects"];
            if (profiles != null)
            {
                List<Tuple<string, string>> parentsLinks = new();
                foreach (var profile in profiles)
                {
                    if (profile is JObject profileObject)
                    {
                        T? newObject = DeserializeObject(profileObject);
                        if (newObject != null)
                        {
                            AddObject(newObject);
                            string? parent = (string?)profileObject["parent"];
                            if (parent != null)
                                parentsLinks.Add(new(parent, newObject.ID));
                        }
                    }
                }
                foreach (var parentsLink in parentsLinks)
                {
                    string parentID = parentsLink.Item1;
                    string childID = parentsLink.Item2;
                    if (m_Objects.TryGetValue(parentID, out T? parent) &&
                        m_Objects.TryGetValue(childID, out T? child))
                        child.SetParent(parent);
                }
            }
            string? currentCommandProfile = (string?)json["current"];
            if (currentCommandProfile != null)
                SetCurrentObject(currentCommandProfile);
        }

        public void FillComboBox(ref ComboBox comboBox)
        {
            comboBox.Items.Clear();
            foreach (T obj in Objects)
            {
                ManagedObject<T>.Info objectInfo = obj.ObjectInfo;
                comboBox.Items.Add(objectInfo);
                if (objectInfo.ID == CurrentObjectID)
                    comboBox.SelectedItem = objectInfo;
            }
        }

        public void Load()
        {
            if (File.Exists(m_FilePath))
            {
                JObject json = JObject.Parse(File.ReadAllText(m_FilePath));
                Deserialize(json);
            }
        }

        public void Save() => File.WriteAllText(m_FilePath, Serialize().ToString());

        protected abstract T? DeserializeObject(JObject obj);
    }
}
