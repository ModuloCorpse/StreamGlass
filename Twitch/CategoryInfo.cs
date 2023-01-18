using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamGlass.Twitch
{
    public class CategoryInfo
    {
        private readonly string m_ID;
        private readonly string m_Name;
        private readonly string m_ImageURL;

        public string ID => m_ID;
        public string Name => m_Name;
        public string ImageURL => m_ImageURL;

        public CategoryInfo(string id, string name, string imageURL)
        {
            m_ID = id;
            m_Name = name;
            m_ImageURL = imageURL;
        }
    }
}
