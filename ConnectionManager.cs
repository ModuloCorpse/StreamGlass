using StreamGlass.StreamChat;
using StreamFeedstock.Controls;
using System.Collections.Generic;

namespace StreamGlass
{
    public class ConnectionManager: IStreamChat
    {
        private readonly List<IConnection> m_Connections = new();
        private readonly List<IStreamChat> m_StreamChatConnections = new();

        public void RegisterConnection(IConnection connection)
        {
            m_Connections.Add(connection);
            if (connection is IStreamChat streamChatConnction)
                m_StreamChatConnections.Add(streamChatConnction);
        }

        public void FillSettings(Settings.Dialog dialog)
        {
            foreach (var connection in m_Connections)
                dialog.AddTabItem(connection.GetSettings());
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
    }
}
