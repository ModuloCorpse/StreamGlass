using StreamGlass.Controls;
using StreamGlass.Profile;
using System.Collections.Generic;

namespace StreamGlass.Connections
{
    public class ConnectionManager
    {
        private readonly List<AConnection> m_Connections = new();
        private readonly List<AStreamConnection> m_StreamChatConnections = new();

        public void RegisterConnection(AConnection connection)
        {
            connection.OnCreate();
            m_Connections.Add(connection);
            if (connection is AStreamConnection streamChatConnection)
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
