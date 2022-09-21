namespace StreamGlass.Twitch
{
    public class UserInfo
    {
        private readonly string m_ID;
        private readonly string m_Login;
        private readonly string m_DisplayName;
        private readonly string m_Type;
        private readonly string m_BroadcasterType;
        private readonly string m_Description;

        public UserInfo(string iD, string login, string displayName, string type, string broadcasterType, string description)
        {
            m_ID = iD;
            m_Login = login;
            m_DisplayName = displayName;
            m_Type = type;
            m_BroadcasterType = broadcasterType;
            m_Description = description;
        }

        public string ID => m_ID;
        public string Login => m_Login;
        public string DisplayName => m_DisplayName;
        public string Type => m_Type;
        public string BroadcasterType => m_BroadcasterType;
        public string Description => m_Description;
    }
}
