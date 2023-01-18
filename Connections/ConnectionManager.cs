using StreamFeedstock.Controls;
using StreamGlass.Profile;
using System.Collections.Generic;
using static StreamGlass.Twitch.IRC.Message;

namespace StreamGlass.Connections
{
    public class ConnectionManager
    {
        private readonly List<IConnection> m_Connections = new();
        private readonly List<IStreamConnection> m_StreamChatConnections = new();

        public void RegisterConnection(IConnection connection)
        {
            m_Connections.Add(connection);
            if (connection is IStreamConnection streamChatConnection)
                m_StreamChatConnections.Add(streamChatConnection);
        }

        public void FillSettings(Settings.Dialog dialog)
        {
            foreach (var connection in m_Connections)
                dialog.AddTabItem(connection.GetSettings());
        }

        public void Test()
        {
            foreach (var connection in m_Connections)
                connection.Test();
        }

        public void Disconnect()
        {
            foreach (var connection in m_Connections)
                connection.Disconnect();
        }

        public void Update(long deltaTime)
        {
            foreach (var connection in m_Connections)
                connection.Update(deltaTime);
        }

        public void SendMessage(string channel, string message)
        {
            foreach (var connection in m_StreamChatConnections)
                connection.SendMessage(channel, message);
        }

        public string GetEmoteURL(string emoteID, BrushPaletteManager palette)
        {
            string url = "";
            foreach (var connection in m_StreamChatConnections)
            {
                url = connection.GetEmoteURL(emoteID, palette);
                if (!string.IsNullOrEmpty(url))
                    return url;
            }
            return url;
        }

        public CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info)
        {
            foreach (var connection in m_StreamChatConnections)
            {
                CategoryInfo? categoryInfo = connection.SearchCategoryInfo(parent, info);
                if (categoryInfo != null)
                    return categoryInfo;
            }
            return null;
        }
    }
}
