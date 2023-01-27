using Quicksand.Web;
using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using StreamFeedstock;
using StreamGlass.Http;
using System;
using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    public class PubSub : AClientListener
    {
        private readonly Settings.Data m_Settings;
        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private OAuthToken? m_Token;
        private string m_ChannelID = "";

        private long m_TimeSinceLastPing = 0;
        private long m_TimeBeforeNextPing = (new Random().Next(-15, 15) + 135) * 1000;

        public PubSub(Settings.Data settings)
        {
            m_Settings = settings;
            m_Websocket = new(this, "wss://pubsub-edge.twitch.tv");
        }

        public void SetToken(OAuthToken token)
        {
            m_Token = token;
        }

        private void ConnectToServer()
        {
            Dictionary<string, string> extensions = new() { { "Authorization", string.Format("Bearer {0}", m_Token!.Token) } };
            m_Websocket.Connect(extensions);
        }

        public void Connect(string channelID)
        {
            m_ChannelID = channelID;
            ConnectToServer();
        }

        private void HandleReconnect()
        {
            Logger.Log("PubSub", "<= Reconnect");
            m_Websocket.Disconnect();
            ConnectToServer();
        }

        public override void OnWebSocketOpen(int clientID, Response response)
        {
            List<string> topics = new() { string.Format("automod-queue.{0}.{0}", m_ChannelID) };
            Json json = new();
            json.Set("type", "LISTEN");
            Json data = new();
            data.Set("topics", topics);
            data.Set("auth_token", m_Token!.Token);
            json.Set("data", data);
            m_Websocket.Send(json.ToNetworkString());
        }

        public override void OnWebSocketMessage(int clientID, string message)
        {
            Logger.Log("PubSub", string.Format("<= {0}", message));
            Json data = new(message);
            if (data.TryGet("type", out string? type))
            {
                switch (type)
                {
                    case "RECONNECT":
                        {
                            HandleReconnect();
                            break;
                        }
                    case "MESSAGE":
                        {

                            break;
                        }
                }
            }
        }

        internal void Update(long deltaTime)
        {
            m_TimeSinceLastPing += deltaTime;
            if (m_TimeSinceLastPing >= m_TimeBeforeNextPing)
            {
                m_Websocket.Send("{\"type\": \"PING\"}");
                m_TimeSinceLastPing = 0;
                m_TimeBeforeNextPing = (new Random().Next(-15, 15) + 135) * 1000; //2"15 +/- 15 seconds
            }
        }

        public override void OnWebSocketClose(int clientID, short code, string closeMessage)
        {
            Logger.Log("PubSub", string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        public override void OnWebSocketError(int clientID, string error)
        {
            Logger.Log("PubSub", string.Format("<=[Error] WebSocket error: {0}", error));
        }

        public override void OnClientDisconnect(int clientID)
        {
            Logger.Log("PubSub", "<= Disconnected");
        }

        public override void OnWebSocketFrame(int clientID, Frame frame)
        {
            Logger.Log("PubSub", string.Format("<=[WS] {0}", frame.ToString().Trim()));
        }

        public override void OnWebSocketFrameSent(int clientID, Frame frame)
        {
            Logger.Log("PubSub", string.Format("[WS]=> {0}", frame.ToString().Trim()));
        }

        public void Disconnect() => m_Websocket.Disconnect();
    }
}
