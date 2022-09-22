using Newtonsoft.Json.Linq;
using System;

namespace StreamGlass
{
    public abstract class ManagedObject<CRTP> where CRTP : ManagedObject<CRTP>
    {
        public class Info
        {
            private readonly string m_ID;
            private readonly string m_Name;
            public Info(string id, string name)
            {
                m_ID = id;
                m_Name = name;
            }
            public string ID => m_ID;
            public string Name => m_Name;
            public override string? ToString() => m_Name;
        }

        private readonly Info m_ObjectInfo;
        private CRTP? m_Parent = default;

        protected ManagedObject(string name)
        {
            m_ObjectInfo = new(Guid.NewGuid().ToString(), name);
        }

        protected ManagedObject(JObject json)
        {
            string? id = (string?)json["id"];
            if (id == null)
                throw new NullReferenceException("Command profile ID is null");
            string? name = (string?)json["name"];
            if (name == null)
                throw new NullReferenceException("Command profile name is null");
            m_ObjectInfo = new(id, name);
            Load(json);
        }

        internal void SetParent(CRTP parent) => m_Parent = parent;

        internal JObject Serialize()
        {
            JObject json = new()
            {
                ["id"] = m_ObjectInfo.ID,
                ["name"] = m_ObjectInfo.Name
            };
            if (m_Parent != null)
                json["parent"] = m_Parent.ID;
            Save(ref json);
            return json;
        }

        abstract protected void Save(ref JObject json);
        abstract protected void Load(JObject json);

        public string ID => m_ObjectInfo.ID;
        public string Name => m_ObjectInfo.Name;
        protected CRTP? Parent => m_Parent;
        public Info ObjectInfo => m_ObjectInfo;
    }
}
