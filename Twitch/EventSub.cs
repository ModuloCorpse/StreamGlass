using Newtonsoft.Json.Linq;
using Quicksand.Web;
using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using System;
using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    internal class EventSub : AClientListener
    {
        public class Metadata
        {
            private readonly string m_ID;
            private readonly string m_Type;
            private readonly string m_Timestamp;
            private readonly string m_Subscription;
            private readonly string m_Version;

            public Metadata(JObject metadata)
            {
                m_ID = metadata.GetValue("message_id")!.ToString();
                m_Type = metadata.GetValue("message_type")!.ToString();
                m_Timestamp = metadata.GetValue("message_timestamp")!.ToString();
                JToken? subscriptionToken = metadata.GetValue("subscription_type");
                m_Subscription = (subscriptionToken != null) ? subscriptionToken.ToString() : "";
                JToken? versionToken = metadata.GetValue("subscription_version");
                m_Version = (versionToken != null) ? versionToken.ToString() : "";
            }

            public string ID => m_ID;
            public string Type => m_Type;
            public string Timestamp => m_Timestamp;
            public string Subscription => m_Subscription;
            public string Version => m_Version;
        }

        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private OAuthToken? m_Token;
        private string m_ChannelID = "";
        private readonly List<string> m_Topics = new();

        public EventSub()
        {
            m_Websocket = new(this, "wss://eventsub-beta.wss.twitch.tv/ws");
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
            m_Topics.Clear();
            m_Topics.Add(string.Format("channel-bits-events-v2.{0}", channelID));
            m_Topics.Add(string.Format("channel-points-channel-v1.{0}", channelID));
            m_Topics.Add(string.Format("channel-subscribe-events-v1.{0}", channelID));
            ConnectToServer();
        }

        public void Disconnect() => m_Websocket.Disconnect();

        private void Reconnect()
        {
            m_Websocket.Disconnect();
            ConnectToServer();
        }

        protected override void OnWebSocketClose(int clientID, short code, string closeMessage)
        {
            Logger.Log("EventSub", string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        protected override void OnWebSocketError(int clientID, string error)
        {
            Logger.Log("EventSub", string.Format("<=[Error] WebSocket error: {0}", error));
        }

        private void RegisterSubscription(string sessionID, string subscriptionName, int subscriptionVersion)
        {
            JObject message = new()
            {
                ["type"] = subscriptionName,
                ["version"] = subscriptionVersion,
                ["condition"] = new JObject()
                {
                    ["broadcaster_user_id"] = m_ChannelID
                },
                ["transport"] = new JObject()
                {
                    ["method"] = "websocket",
                    ["session_id"] = sessionID
                }
            };
            ManagedRequest request = new PostRequest("https://api.twitch.tv/helix/eventsub/subscriptions", message.ToString(Newtonsoft.Json.Formatting.None))
                .AddHeaderField("Content-Type", "application/json; charset=utf-8")
                .AddHeaderField("Authorization", string.Format("Bearer {0}", m_Token!.Token))
                .AddHeaderField("Client-Id", m_Token!.ClientID);
            Logger.Log("EventSub", string.Format("[HTTP]=> {0}", request.ToString().Trim()));
            Response? response = request.Send();
            if (response != null)
                Logger.Log("EventSub", string.Format("<=[HTTP] {0}", response.ToString().Trim()));
            if (response != null && response.StatusCode == 401)
            {
                m_Token.Refresh();
                request.Reset();
                Logger.Log("EventSub", string.Format("[HTTP]=> {0}", request.ToString().Trim()));
                response = request.Send();
                if (response != null)
                    Logger.Log("EventSub", string.Format("<=[HTTP] {0}", response.ToString().Trim()));
            }
            if (response != null && response.StatusCode == 202)
                Logger.Log("EventSub", string.Format("<=[HTTP] Listening to {0}", subscriptionName));
        }

        private void HandleWelcome(JObject payload)
        {
            JObject? sessionObj = (JObject?)payload["session"];
            if (sessionObj != null)
            {
                string? sessionID = (string?)sessionObj["id"];
                if (sessionID != null)
                {
                    RegisterSubscription(sessionID, "channel.follow", 1);
                    RegisterSubscription(sessionID, "channel.subscribe", 1);
                    RegisterSubscription(sessionID, "channel.cheer", 1);
                    RegisterSubscription(sessionID, "channel.raid", 1);
                    RegisterSubscription(sessionID, "channel.channel_points_custom_reward_redemption.add", 1);
                    RegisterSubscription(sessionID, "stream.online", 1);
                }
            }
        }

        protected override void OnWebSocketMessage(int clientID, string message)
        {
            Logger.Log("EventSub", string.Format("<= {0}", message.Trim()));
            if (string.IsNullOrEmpty(message))
                return;
            try
            {
                JObject eventMessage = JObject.Parse(message);
                JObject? metadataObj = (JObject?)eventMessage["metadata"];
                JObject? payload = (JObject?)eventMessage["payload"];
                if (metadataObj != null && payload != null)
                {
                    Metadata metadata = new(metadataObj);
                    switch (metadata.Type)
                    {
                        case "session_welcome": HandleWelcome(payload); break;
                        case "session_reconnect": Reconnect(); break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("EventSub", string.Format("[Error]=> Json Exception: {0}", e));
                return;
            }
        }

        protected override void OnWebSocketOpen(int clientID, Response response)
        {
            Logger.Log("EventSub", string.Format("<=[HTTP] {0}", response.ToString().Trim()));
        }

        protected override void OnHttpRequestSent(int clientID, Request request)
        {
            Logger.Log("EventSub", string.Format("[HTTP]=> {0}", request.ToString().Trim()));
        }

        protected override void OnClientDisconnect(int clientID)
        {
            Logger.Log("EventSub", "<=[HTTP] Disconnected");
        }
    }
}
