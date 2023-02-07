using Quicksand.Web;
using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using StreamFeedstock;
using StreamFeedstock.StructuredText;
using StreamGlass.Http;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using static StreamGlass.Twitch.EventSub.EventData;

namespace StreamGlass.Twitch
{
    public class PubSub : AClientListener
    {
        private readonly API m_API;
        private readonly Settings.Data m_Settings;
        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private OAuthToken? m_Token;
        private string m_ChannelID = "";

        private long m_TimeSinceLastPing = 0;
        private long m_TimeBeforeNextPing = (new Random().Next(-15, 15) + 135) * 1000;

        public PubSub(API api, Settings.Data settings)
        {
            m_API = api;
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
            Log.Str("PubSub", "<= Reconnect");
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

        private static Text GetMessage(Json messageData)
        {
            string messageText = messageData.GetOrDefault("text", "");
            List<Json> fragmentsObject = messageData.GetList<Json>("fragments");
            List<string> fragments = new();
            foreach (Json fragment in fragmentsObject)
            {
                if (fragment.TryGet("text", out string? str))
                    fragments.Add(str!);
            }
            return new(messageText);
        }

        private void HandleAutoModQueueData(Json json)
        {
            if (json.TryGet("data", out Json? data))
            {
                if (data!.TryGet("status", out string? status) &&
                    data!.TryGet("message", out Json? message))
                {
                    if (status == "PENDING" &&
                        message!.TryGet("id", out string? messageID) &&
                        message.TryGet("content", out Json? messageData) &&
                        message.TryGet("sender", out Json? messageSender))
                    {
                        if (messageSender!.TryGet("login", out string? login))
                        {
                            string color = messageSender.GetOrDefault("chat_color", "");
                            User? sender = m_API.GetUserInfoFromLogin(login!);
                            if (sender != null)
                                CanalManager.Emit(StreamGlassCanals.HELD_MESSAGE, new UserMessage(sender, false, messageID!, color!, "", GetMessage(messageData!)));
                        }
                    }
                }
            }
        }

        public override void OnWebSocketMessage(int clientID, string message)
        {
            Log.Str("PubSub", string.Format("<= {0}", message));
            Json receivedEvent = new(message);
            if (receivedEvent.TryGet("type", out string? type))
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
                            if (receivedEvent.TryGet("data", out Json? data))
                            {
                                if (data!.TryGet("topic", out string? topic) &&
                                    data.TryGet("message", out string? messageDataStr))
                                {
                                    Json messageData = new(messageDataStr!);
                                    if (topic!.StartsWith("automod-queue"))
                                        HandleAutoModQueueData(messageData!);
                                }
                            }
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
            Log.Str("PubSub", string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        public override void OnWebSocketError(int clientID, string error)
        {
            Log.Str("PubSub", string.Format("<=[Error] WebSocket error: {0}", error));
        }

        public override void OnClientDisconnect(int clientID)
        {
            Log.Str("PubSub", "<= Disconnected");
        }

        public override void OnWebSocketFrame(int clientID, Frame frame)
        {
            Log.Str("PubSub", string.Format("<=[WS] {0}", frame.ToString().Trim()));
        }

        public override void OnWebSocketFrameSent(int clientID, Frame frame)
        {
            Log.Str("PubSub", string.Format("[WS]=> {0}", frame.ToString().Trim()));
        }

        public void Disconnect() => m_Websocket.Disconnect();
    }
}
