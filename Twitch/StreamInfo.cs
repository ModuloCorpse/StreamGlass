namespace StreamGlass.Twitch
{
    public class StreamInfo
    {
        private readonly UserInfo m_Broadcaster;
        private readonly string m_ID;
        private readonly string m_GameID;
        private readonly string m_GameName;
        private readonly string m_Type;
        private readonly string m_Title;
        private readonly string m_Language;
        private readonly int m_Viewers;
        private readonly bool m_IsMature;

        public StreamInfo(UserInfo broadcaster, string id, string gameID, string gameName, string type, string title, string language, int viewers, bool isMature)
        {
            m_Broadcaster = broadcaster;
            m_ID = id;
            m_GameID = gameID;
            m_GameName = gameName;
            m_Type = type;
            m_Title = title;
            m_Language = language;
            m_Viewers = viewers;
            m_IsMature = isMature;
        }

        public UserInfo Broadcaster => m_Broadcaster;
        public string ID => m_ID;
        public string GameID => m_GameID;
        public string GameName => m_GameName;
        public string Type => m_Type;
        public string Title => m_Title;
        public string Language => m_Language;
        public int Viewers => m_Viewers;
        public bool IsMature => m_IsMature;
    }
}
