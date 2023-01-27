using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    public class APICache
    {
        private readonly Dictionary<string, EmoteInfo> m_Emotes = new();
        private readonly Dictionary<string, User> m_UserInfoFromLogin = new();
        private readonly Dictionary<string, User> m_UserInfoFromID = new();

        internal void AddEmote(EmoteInfo info)
        {
            m_Emotes[info.ID] = info;
        }

        public EmoteInfo? GetEmote(string id)
        {
            if (m_Emotes.TryGetValue(id, out EmoteInfo? info))
                return info;
            return null;
        }

        internal void AddUserInfo(User info)
        {
            m_UserInfoFromID[info.ID] = info;
            m_UserInfoFromLogin[info.Name] = info;
        }

        public User? GetUserInfoByLogin(string login)
        {
            if (m_UserInfoFromLogin.TryGetValue(login, out User? info))
                return info;
            return null;
        }
    }
}
