using System;

namespace StreamGlass.ManagedObject
{
    public abstract class Object<CRTP> where CRTP : Object<CRTP>
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
            public override string ToString() => m_Name;
        }

        private readonly bool m_IsSerializable = true;
        private readonly Info m_ObjectInfo;
        private CRTP? m_Parent = default;

        protected Object(string name, bool isSerializable = true)
        {
            m_ObjectInfo = new(Guid.NewGuid().ToString(), name);
            m_IsSerializable = isSerializable;
        }

        protected Object(string id, string name, bool isSerializable = true)
        {
            m_ObjectInfo = new(id, name);
            m_IsSerializable = isSerializable;
        }

        protected Object(Json json)
        {
            string? id = json.Get<string>("id");
            id ??= Guid.NewGuid().ToString();
            string? name = json.Get<string>("name");
            if (name == null)
                throw new NullReferenceException("ManagedObject name is null");
            m_ObjectInfo = new(id, name);
            Load(json);
        }

        internal bool IsSerializable() => m_IsSerializable;

        internal void SetParent(CRTP parent) => m_Parent = parent;

        internal Json Serialize()
        {
            Json json = new();
            json.Set("id", ID);
            json.Set("name", Name);
            if (m_Parent != null)
                json.Set("parent", m_Parent.ID);
            Save(ref json);
            return json;
        }



        abstract protected void Save(ref Json json);
        abstract protected void Load(Json json);


        public override bool Equals(object? obj) => (obj != null && obj is Object<CRTP> other && other.ID == ID);

        public override int GetHashCode() => HashCode.Combine(ID);

        public string ID => m_ObjectInfo.ID;
        public string Name => m_ObjectInfo.Name;
        public CRTP? Parent => m_Parent;
        public Info ObjectInfo => m_ObjectInfo;
    }
}
