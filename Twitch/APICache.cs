using System;
using System.Collections.Generic;
using System.Text;

namespace StreamGlass.Twitch
{
    public class APICache
    {
        private readonly Dictionary<string, ChannelInfo> m_ChannelInfoFromLogin = new();
        private readonly Dictionary<string, ChannelInfo> m_ChannelInfoFromID = new();
        private readonly Dictionary<string, StreamInfo> m_StreamInfoFromLogin = new();
        private readonly Dictionary<string, StreamInfo> m_StreamInfoFromID = new();
        private readonly Dictionary<string, UserInfo> m_UserInfoFromLogin = new();
        private readonly Dictionary<string, UserInfo> m_UserInfoFromID = new();

        internal void AddChannelInfo(ChannelInfo info)
        {
            m_ChannelInfoFromID[info.Broadcaster.ID] = info;
            m_ChannelInfoFromLogin[info.Broadcaster.Login] = info;
        }

        public ChannelInfo? GetChannelInfoByID(string id)
        {
            if (m_ChannelInfoFromID.TryGetValue(id, out ChannelInfo? info))
                return info;
            return null;
        }

        public ChannelInfo? GetChannelInfoByLogin(string login)
        {
            if (m_ChannelInfoFromLogin.TryGetValue(login, out ChannelInfo? info))
                return info;
            return null;
        }

        internal void AddStreamInfo(StreamInfo info)
        {
            m_StreamInfoFromID[info.Broadcaster.ID] = info;
            m_StreamInfoFromLogin[info.Broadcaster.Login] = info;
        }

        public StreamInfo? GetStreamInfoByID(string id)
        {
            if (m_StreamInfoFromID.TryGetValue(id, out StreamInfo? info))
                return info;
            return null;
        }

        public StreamInfo? GetStreamInfoByLogin(string login)
        {
            if (m_StreamInfoFromLogin.TryGetValue(login, out StreamInfo? info))
                return info;
            return null;
        }

        internal void AddUserInfo(UserInfo info)
        {
            m_UserInfoFromID[info.ID] = info;
            m_UserInfoFromLogin[info.Login] = info;
        }

        public UserInfo? GetUserInfoByID(string id)
        {
            if (m_UserInfoFromID.TryGetValue(id, out UserInfo? info))
                return info;
            return null;
        }

        public UserInfo? GetUserInfoByLogin(string login)
        {
            if (m_UserInfoFromLogin.TryGetValue(login, out UserInfo? info))
                return info;
            return null;
        }
    }
}
