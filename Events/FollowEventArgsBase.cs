using CorpseLib.StructuredText;

namespace StreamGlass.Events
{
    public class FollowEventArgsBase
    {
        private readonly Text m_Message;
        private readonly string m_Name;
        private readonly int m_Tier;

        public Text Message => m_Message;
        public string Name => m_Name;
        public int Tier => m_Tier;

        public FollowEventArgsBase(string name, Text message, int tier)
        {
            m_Name = name;
            m_Message = message;
            m_Tier = tier;
        }
    }
}
