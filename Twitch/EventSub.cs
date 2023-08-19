using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Logging;
using CorpseLib.Network;
using CorpseLib.Web;
using CorpseLib.Web.Http;
using CorpseLib.Web.OAuth;
using Quicksand.Web;
using StreamGlass.Events;
using System;
using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    public class EventSub : AClientListener
    {
        public static readonly Logger EVENTSUB = new("[${d}-${M}-${y} ${h}:${m}:${s}.${ms}] ${log}") { new LogInFile("./log/${y}${M}${d}${h}-EventSub.log") };

        public class Metadata
        {
            private readonly string m_ID;
            private readonly string m_Type;
            private readonly string m_Timestamp;
            private readonly string m_Subscription;
            private readonly string m_Version;

            public string ID => m_ID;
            public string Type => m_Type;
            public string Timestamp => m_Timestamp;
            public string Subscription => m_Subscription;
            public string Version => m_Version;

            public Metadata(JObject obj)
            {
                m_ID = obj.Get<string>("message_id")!;
                m_Type = obj.Get<string>("message_type")!;
                m_Timestamp = obj.Get<string>("message_timestamp")!;
                m_Subscription = obj.GetOrDefault("subscription_type", "")!;
                m_Version = obj.GetOrDefault("subscription_version", "")!;
            }
        }

        public class Transport
        {
            private readonly string m_Method;
            private readonly string m_SessionID;
            private readonly string m_Callback;
            private readonly string m_Secret;

            public string Method => m_Method;
            public string SessionID => m_SessionID;
            public string Callback => m_Callback;
            public string Secret => m_Secret;

            public Transport(JObject obj)
            {
                m_Method = obj.Get<string>("method")!;
                if (m_Method == "websocket")
                {
                    m_Callback = "";
                    m_Secret = "";
                    m_SessionID = obj.Get<string>("session_id")!;
                }
                else
                {
                    m_SessionID = "";
                    m_Callback = obj.Get<string>("callback")!;
                    m_Secret = obj.GetOrDefault("secret", "")!;
                }
            }
        }

        public class Subscription
        {
            private readonly Dictionary<string, string> m_Conditions = new();
            private readonly Transport m_Transport;
            private readonly string m_ID;
            private readonly string m_Type;
            private readonly string m_Version;
            private readonly string m_Status;
            private readonly string m_CreatedAt;
            private readonly int m_Cost;

            public Transport Transport => m_Transport;
            public string ID => m_ID;
            public string Type => m_Type;
            public string Version => m_Version;
            public string Status => m_Status;
            public string CreatedAt => m_CreatedAt;
            public int Cost => m_Cost;

            public Subscription(JObject obj)
            {
                m_ID = obj.Get<string>("id")!;
                m_Type = obj.Get<string>("type")!;
                m_Version = obj.Get<string>("version")!;
                m_Status = obj.Get<string>("status")!;
                m_Cost = obj.Get<int>("cost")!;
                m_CreatedAt = obj.Get<string>("created_at")!;
                m_Transport = new(obj.Get<JObject>("transport")!);
                JObject conditionObject = obj.Get<JObject>("condition")!;
                foreach (var pair in conditionObject)
                    m_Conditions[pair.Key] = pair.Value.Cast<string>()!;
            }

            public bool HaveCondition(string condition) => m_Conditions.ContainsKey(condition);
            public string GetCondition(string condition) => m_Conditions[condition];
        }

        public class EventData
        {
            public class User
            {
                private readonly string m_ID;
                private readonly string m_Login;
                private readonly string m_Name;

                public string ID => m_ID;
                public string Login => m_Login;
                public string Name => m_Name;

                public User(string id, string login, string name)
                {
                    m_ID = id;
                    m_Login = login;
                    m_Name = name;
                }
            }

            private readonly JObject m_Data;

            public EventData(JObject data) { m_Data = data; }

            public User? GetUser(string user = "")
            {
                if (m_Data.TryGet(string.Format("{0}user_id", user), out string? id) &&
                    m_Data.TryGet(string.Format("{0}user_login", user), out string? login) &&
                    m_Data.TryGet(string.Format("{0}user_name", user), out string? name))
                    return new(id!, login!, name!);
                return null;
            }

            public T GetOrDefault<T>(string key, T defaultValue) => m_Data.GetOrDefault(key, defaultValue)!;

            public bool TryGet<T>(string key, out T? ret) => m_Data.TryGet(key, out ret);
        }

        private readonly IniSection m_Settings;
        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private RefreshToken? m_Token;
        private readonly HashSet<string> m_TreatedMessage = new();
        private string m_ChannelID = "";

        public EventSub(IniSection settings)
        {
            EVENTSUB.Start();
            m_Settings = settings;
            m_Websocket = new(this, "wss://eventsub.wss.twitch.tv/ws");
        }

        public void SetToken(RefreshToken token)
        {
            m_Token = token;
        }

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

        public void Disconnect() => m_Websocket.Disconnect();

        internal void Reconnect()
        {
            m_Websocket.Disconnect();
            EVENTSUB.Log("<= Reconnecting");
            ConnectToServer();
        }

        public override void OnWebSocketClose(int clientID, short code, string closeMessage)
        {
            EVENTSUB.Log(string.Format("<=[Error] WebSocket closed ({0}): {1}", code, closeMessage));
        }

        public override void OnWebSocketError(int clientID, string error)
        {
            EVENTSUB.Log(string.Format("<=[Error] WebSocket error: {0}", error));
        }

        private bool RegisterSubscription(string sessionID, string subscriptionName, int subscriptionVersion, params string[] conditions)
        {
            if (m_Token == null)
                return false;
            JObject transportJson = new();
            transportJson.Set("method", "websocket");
            transportJson.Set("session_id", sessionID);
            JObject conditionJson = new();
            foreach (string condition in conditions)
                conditionJson.Set(condition, m_ChannelID);
            JObject message = new();
            message.Set("type", subscriptionName);
            message.Set("version", subscriptionVersion);
            message.Set("condition", conditionJson);
            message.Set("transport", transportJson);
            URLRequest request = new(URI.Parse("https://api.twitch.tv/helix/eventsub/subscriptions"), Request.MethodType.POST, message.ToNetworkString());
            request.AddContentType(MIME.APPLICATION.JSON);
            request.AddRefreshToken(m_Token);
            EVENTSUB.Log(string.Format("Sending: {0}", request.Request.ToString()));
            Response response = request.Send();
            EVENTSUB.Log(string.Format("Received: {0}", response.ToString()));
            if (response.StatusCode == 202)
            {
                EVENTSUB.Log(string.Format("<= Listening to {0}", subscriptionName));
                return true;
            }
            else
            {
                EVENTSUB.Log(string.Format("<= Error when listening to {0}: {1}", subscriptionName, response.Body));
                return false;
            }
        }

        private void HandleWelcome(JObject payload)
        {
            EVENTSUB.Log(string.Format("<= Welcome: {0}", payload));
            if (payload.TryGet("session", out JObject? sessionObj))
            {
                if (sessionObj!.TryGet("id", out string? sessionID))
                {
                    if (!RegisterSubscription(sessionID!, "channel.follow", 2, "broadcaster_user_id", "moderator_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "channel.subscribe", 1, "broadcaster_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "channel.subscription.gift", 1, "broadcaster_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "channel.raid", 1, "to_broadcaster_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "channel.raid", 1, "from_broadcaster_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "channel.channel_points_custom_reward_redemption.add", 1, "broadcaster_user_id"))
                        return;
                    if (!RegisterSubscription(sessionID!, "stream.online", 1, "broadcaster_user_id"))
                        return;
                    RegisterSubscription(sessionID!, "stream.offline", 1, "broadcaster_user_id");
                }
            }
        }

        private static void HandleFollow(EventData data)
        {
            EventData.User? follower = data.GetUser();
            if (follower != null)
                StreamGlassCanals.FOLLOW.Emit(new(follower.Name, new(""), 0, -1, -1));
        }

        private void HandleSub(EventData data)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            int followTier;
            EventData.User? follower = data.GetUser();
            if (follower != null && data.TryGet("tier", out string? tier))
            {
                bool isGift = data.GetOrDefault("is_gift", false);
                switch (tier!)
                {
                    case "1000": followTier = 1; break;
                    case "2000": followTier = 2; break;
                    case "3000": followTier = 3; break;
                    default: return;
                }
                if (isGift)
                    StreamGlassCanals.GIFT_FOLLOW.Emit(new(string.Empty, follower.Name, new(""), followTier, -1, -1, -1));
                else
                    StreamGlassCanals.FOLLOW.Emit(new(follower.Name, new(""), followTier, -1, -1));
            }
        }

        private void HandleSubGift(EventData data)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            int followTier;
            if (data.TryGet("is_anonymous", out bool? isAnonymous))
            {
                EventData.User? follower = data.GetUser();
                if (((bool)isAnonymous! || follower != null) && data.TryGet("tier", out string? tier) && data.TryGet("total", out int? nbGift))
                {
                    switch (tier!)
                    {
                        case "1000": followTier = 1; break;
                        case "2000": followTier = 2; break;
                        case "3000": followTier = 3; break;
                        default: return;
                    }
                    StreamGlassCanals.GIFT_FOLLOW.Emit(new(string.Empty, (follower != null) ? follower.Name : "", new(""), followTier, -1, -1, (int)nbGift!));
                }
            }
        }

        private static void HandleReward(EventData data)
        {
            EventData.User? viewer = data.GetUser();
            if (viewer != null && data.TryGet("reward", out JObject? rewardInfo))
            {
                data.TryGet("user_input", out string? input);
                if (rewardInfo!.TryGet("title", out string? title))
                    StreamGlassCanals.REWARD.Emit(new(viewer.ID, viewer.Name, title!, input ?? ""));
            }
        }

        private static void HandleRaid(EventData data, bool incomming)
        {
            EventData.User? from = data.GetUser("from_broadcaster_");
            EventData.User? to = data.GetUser("to_broadcaster_");
            if (from != null && to != null && data.TryGet("viewers", out int? viewers))
                StreamGlassCanals.RAID.Emit(new RaidEventArgs(from.ID, from.Name, to.ID, to.Name, (int)viewers!, incomming));
        }

        private static void HandleStreamStart() => StreamGlassCanals.STREAM_START.Trigger();

        private static void HandleStreamStop() => StreamGlassCanals.STREAM_STOP.Trigger();

        private static bool IsIncommingRaid(Subscription subscription) => !(subscription.HaveCondition("from_broadcaster_user_id") && !string.IsNullOrEmpty(subscription.GetCondition("from_broadcaster_user_id")));

        private void HandleNotification(Metadata metadata, JObject payload, string message)
        {
            EVENTSUB.Log(string.Format("<= {0}", metadata.Subscription));
            if (payload.TryGet("subscription", out JObject? subscriptionObj) &&
                payload.TryGet("event", out JObject? eventObj))
            {
                try
                {
                    Subscription subscription = new(subscriptionObj!);
                    EventData eventData = new(eventObj!);
                    switch (subscription.Type)
                    {
                        case "channel.follow": HandleFollow(eventData); break;
                        case "channel.subscribe": HandleSub(eventData); break;
                        case "channel.subscription.gift": HandleSubGift(eventData); break;
                        case "channel.raid": HandleRaid(eventData, IsIncommingRaid(subscription)); break;
                        case "channel.channel_points_custom_reward_redemption.add": HandleReward(eventData); break;
                        case "stream.online": HandleStreamStart(); break;
                        case "stream.offline": HandleStreamStop(); break;
                        default: EVENTSUB.Log(string.Format("<= {0}", message)); break;
                    }
                }
                catch (Exception e)
                {
                    EVENTSUB.Log(string.Format("<= Exception when treating received message: {0}", e.Message));
                }
            }
        }

        public override void OnWebSocketMessage(int clientID, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            try
            {
                JFile eventMessage = new(message);
                if (eventMessage.TryGet("metadata", out JObject? metadataObj) &&
                    eventMessage.TryGet("payload", out JObject? payload))
                {
                    try
                    {
                        Metadata metadata = new(metadataObj!);
                        if (!m_TreatedMessage.Contains(metadata.ID))
                        {
                            m_TreatedMessage.Add(metadata.ID);
                            switch (metadata.Type)
                            {
                                case "session_welcome": HandleWelcome(payload!); break;
                                case "session_keepalive": break;
                                case "notification": HandleNotification(metadata, payload!, message.Trim()); break;
                                case "session_reconnect": Reconnect(); break;
                                case "revocation": break;
                                default: EVENTSUB.Log(string.Format("<= {0}", message.Trim())); break;
                            }
                        }
                    } catch (Exception e)
                    {
                        EVENTSUB.Log(string.Format("[Error]=> Treatment Exception: {0}", e.Message));
                    }
                }
            }
            catch (Exception e)
            {
                EVENTSUB.Log(string.Format("[Error]=> Json Exception: {0}", e));
                return;
            }
        }

        public override void OnClientDisconnect(int clientID)
        {
            EVENTSUB.Log("<= Disconnected");
        }

        public override void OnWebSocketOpen(int clientID, Quicksand.Web.Http.Response response)
        {
            EVENTSUB.Log("<= Connected");
        }
    }
}
