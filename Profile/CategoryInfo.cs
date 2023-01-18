namespace StreamGlass.Profile
{
    public class CategoryInfo
    {
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
