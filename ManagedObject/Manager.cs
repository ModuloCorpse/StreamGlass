using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace StreamGlass.ManagedObject
{
    public abstract class Manager<T> where T : Object<T>
    {
        private readonly string m_DirPath;
        private readonly Dictionary<string, T> m_Objects = new();
        private T? m_CurrentObject = null;

        protected Manager(string directoryPath) => m_DirPath = directoryPath;

        public T? GetObject(string id)
        {
            if (m_Objects.TryGetValue(id, out T? obj))
                return obj;
            return null;
        }

        protected void SetObject(T obj)
        {
            if (obj.ID == CurrentObjectID)
                m_CurrentObject = obj;
            m_Objects[obj.ID] = obj;
        }

        protected void AddObject(T obj) => m_Objects[obj.ID] = obj;

        internal void RemoveObject(string id)
        {
            if (m_Objects.ContainsKey(id))
            {
                m_Objects.Remove(id);
                File.Delete(string.Format("{0}/{1}.json", m_DirPath, id));
                if (m_CurrentObject != null && m_CurrentObject.ID == id)
                    m_CurrentObject = null;
            }
        }

        protected bool SetCurrentObject(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                m_CurrentObject = null;
                return true;
            }
            else if (m_Objects.TryGetValue(id, out T? obj))
            {
                m_CurrentObject = obj;
                return true;
            }
            return false;
        }

        protected T? CurrentObject => m_CurrentObject;
        public System.Collections.Generic.List<T> Objects => m_Objects.Values.ToList();
        public System.Collections.Generic.List<Object<T>.Info> ObjectsInfo => m_Objects.Select(pair => pair.Value.ObjectInfo).ToList();
        public string CurrentObjectID => (m_CurrentObject != null) ? m_CurrentObject.ID : "";

        public void FillComboBox(ref ComboBox comboBox, bool addEmpty = true)
        {
            comboBox.Items.Clear();
            if (addEmpty)
            {
                Object<T>.Info info = new("", "");
                comboBox.Items.Add(info);
                if (CurrentObject == null)
                    comboBox.SelectedItem = info;
            }
            foreach (T obj in Objects)
            {
                Object<T>.Info objectInfo = obj.ObjectInfo;
                comboBox.Items.Add(objectInfo);
                if (objectInfo.ID == CurrentObjectID)
                    comboBox.SelectedItem = objectInfo;
            }
        }

        public void Load()
        {
            if (!Directory.Exists(m_DirPath))
                Directory.CreateDirectory(m_DirPath);
            else
            {
                System.Collections.Generic.List<Tuple<string, string>> parentsLinks = new();
                string[] files = Directory.GetFiles(m_DirPath);
                string? currentID = null;
                foreach (string file in files)
                {
                    try
                    {
                        Json json = Json.LoadFromFile(file);
                        if (file.EndsWith("settings.json"))
                            currentID = json.Get<string>("current");
                        else
                        {
                            T? newObject = DeserializeObject(json);
                            if (newObject != null)
                            {
                                AddObject(newObject);
                                string? parent = json.Get<string>("parent");
                                if (parent != null)
                                    parentsLinks.Add(new(parent, newObject.ID));
                            }
                        }
                    } catch {}
                }
                foreach (var parentsLink in parentsLinks)
                {
                    string parentID = parentsLink.Item1;
                    string childID = parentsLink.Item2;
                    if (m_Objects.TryGetValue(parentID, out T? parent) && m_Objects.TryGetValue(childID, out T? child))
                        child.SetParent(parent);
                }
                if (currentID != null)
                    SetCurrentObject(currentID);
            }
        }

        public void Save()
        {
            if (!Directory.Exists(m_DirPath))
                Directory.CreateDirectory(m_DirPath);

            Json json = new();
            json.Set("current", m_CurrentObject?.ID ?? "");
            json.WriteToFile(string.Format("{0}/settings.json", m_DirPath));

            foreach (var obj in m_Objects.Values)
            {
                if (obj.IsSerializable())
                    obj.Serialize().WriteToFile(string.Format("{0}/{1}.json", m_DirPath, obj.ID));
            }
        }

        public void LinkParentChild(string parentID, string childID)
        {
            if (m_Objects.TryGetValue(parentID, out T? parent) && m_Objects.TryGetValue(childID, out T? child))
                child.SetParent(parent);
        }

        protected abstract T? DeserializeObject(Json obj);
    }
}
