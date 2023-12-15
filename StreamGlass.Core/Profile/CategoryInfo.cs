using CorpseLib.Json;
using CorpseLib;

namespace StreamGlass.Core.Profile
{
    public class CategoryInfo
    {
        public class JSerializer : AJSerializer<CategoryInfo>
        {
            protected override OperationResult<CategoryInfo> Deserialize(JObject reader)
            {
                if (reader.TryGet("id", out string? id) &&
                    reader.TryGet("name", out string? name))
                    return new(new(id!, name!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(CategoryInfo obj, JObject writer)
            {
                writer["id"] = obj.m_ID;
                writer["name"] = obj.m_Name;
            }
        }
        private string m_ID;
        private string m_Name;

        public string ID => m_ID;
        public string Name => m_Name;

        public CategoryInfo(string id, string name)
        {
            m_ID = id;
            m_Name = name;
        }

        public CategoryInfo(string name)
        {
            m_ID = name;
            m_Name = name;
        }

        public void Copy(CategoryInfo other)
        {
            m_ID = other.m_ID;
            m_Name = other.m_Name;
        }

        public void SetID(string id) => m_ID = id;
        public void SetName(string name) => m_Name = name;
    }
}
