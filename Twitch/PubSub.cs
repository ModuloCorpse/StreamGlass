using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.StructuredText;
using CorpseLib.Web.OAuth;
using Quicksand.Web;
using Quicksand.Web.Http;
using System;
using System.Collections.Generic;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class PubSub : AClientListener
    {
        public static readonly Logger PUBSUB = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}-PubSub.log") };

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private API m_API = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private RefreshToken? m_Token;
        private string m_ChannelID = "";

        private long m_TimeSinceLastPing = 0;
        private long m_TimeBeforeNextPing = (new Random().Next(-15, 15) + 135) * 1000;

        public PubSub()
        {
            PUBSUB.Start();
            m_Websocket = new(this, "wss://pubsub-edge.twitch.tv");
        }

        public void SetAPI(API api) => m_API = api;

        public void SetToken(RefreshToken token) => m_Token = token;

        private void ConnectToServer()
        {
            Dictionary<string, string> extensions = new() { { "Authorization", string.Format("Bearer {0}", m_Token!.AccessToken) } };
            m_Websocket.Connect(extensions);
        }

        public void Connect(string channelID)
        {
            m_ChannelID = channelID;
            ConnectToServer();
        }

        private void HandleReconnect()
        {
            PUBSUB.Log("<= Reconnect");
            m_Websocket.Disconnect();
            ConnectToServer();
        }

        public override void OnWebSocketOpen(int clientID, Response response)
        {
            List<string> topics = new() { string.Format("automod-queue.{0}.{0}", m_ChannelID) };
            JObject json = new();
            json.Set("type", "LISTEN");
            JObject data = new();
            data.Set("topics", topics);
            data.Set("auth_token", m_Token!.AccessToken);
            json.Set("data", data);
            m_Websocket.Send(json.ToNetworkString());
        }

        private static Text GetMessage(JObject messageData)
        {
            string messageText = messageData.GetOrDefault("text", "")!;
            List<JObject> fragmentsObject = messageData.GetList<JObject>("fragments");
            List<string> fragments = new();
            foreach (JObject fragment in fragmentsObject)
            {
                if (fragment.TryGet("text", out string? str))
                    fragments.Add(str!);
            }
            return new(messageText);
        }

        private void HandleAutoModQueueData(JObject json)
        {
            if (json.TryGet("data", out JObject? data))
            {
                if (data!.TryGet("status", out string? status) &&
                    data!.TryGet("message", out JObject? message))
                {
                    if (status == "PENDING" &&
                        message!.TryGet("id", out string? messageID) &&
                        message.TryGet("content", out JObject? messageData) &&
                        message.TryGet("sender", out JObject? messageSender))
                    {
                        if (messageSender!.TryGet("login", out string? login))
                        {
                            string color = messageSender.GetOrDefault("chat_color", "")!;
                            User? sender = m_API.GetUserInfoFromLogin(login!);
                            if (sender != null)
                                StreamGlassCanals.HELD_MESSAGE.Emit(new(sender, false, messageID!, color!, "", GetMessage(messageData!)));
                        }
                    }
                    else if ((status == "ALLOWED" || status == "DENIED") && message!.TryGet("id", out string? moderatedMessageID))
                        StreamGlassCanals.HELD_MESSAGE_MODERATED.Emit(moderatedMessageID!);
                }
            }
        }

        public override void OnWebSocketMessage(int clientID, string message)
        {
            PUBSUB.Log(string.Format("<= {0}", message));
            JFile receivedEvent = new(message);
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
                            if (receivedEvent.TryGet("data", out JObject? data))
                            {
                                if (data!.TryGet("topic", out string? topic) &&
                                    data.TryGet("message", out string? messageDataStr))
                                {
                                    JFile messageData = new(messageDataStr!);
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
            PUBSUB.Log(string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        public override void OnWebSocketError(int clientID, string error)
        {
            PUBSUB.Log(string.Format("<=[Error] WebSocket error: {0}", error));
        }

        public override void OnClientDisconnect(int clientID)
        {
            PUBSUB.Log("<= Disconnected");
        }

        public void Disconnect() => m_Websocket.Disconnect();
    }
}
