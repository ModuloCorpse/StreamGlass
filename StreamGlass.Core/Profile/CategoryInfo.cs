using CorpseLib;
using CorpseLib.DataNotation;

namespace StreamGlass.Core.Profile
{
    public class CategoryInfo(string id, string name)
    {
        public class DataSerializer : ADataSerializer<CategoryInfo>
        {
            protected override OperationResult<CategoryInfo> Deserialize(DataObject reader)
            {
                if (reader.TryGet("id", out string? id) &&
                    reader.TryGet("name", out string? name))
                    return new(new(id!, name!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(CategoryInfo obj, DataObject writer)
            {
                writer["id"] = obj.m_ID;
                writer["name"] = obj.m_Name;
            }
        }
        private string m_ID = id;
        private string m_Name = name;

        public string ID => m_ID;
        public string Name => m_Name;

        public CategoryInfo(string name) : this(name, name) { }

        public void Copy(CategoryInfo other)
        {
            m_ID = other.m_ID;
            m_Name = other.m_Name;
        }

        public void SetID(string id) => m_ID = id;
        public void SetName(string name) => m_Name = name;
    }
}
