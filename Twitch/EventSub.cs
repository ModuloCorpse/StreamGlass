using Quicksand.Web;
using Quicksand.Web.Http;
using Quicksand.Web.WebSocket;
using System;
using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    public class EventSub : AClientListener
    {
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

            public Metadata(Json obj)
            {
                m_ID = obj.Get<string>("message_id")!;
                m_Type = obj.Get<string>("message_type")!;
                m_Timestamp = obj.Get<string>("message_timestamp")!;
                m_Subscription = obj.GetOrDefault("subscription_type", "");
                m_Version = obj.GetOrDefault("subscription_version", "");
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

            public Transport(Json obj)
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
                    m_Secret = obj.GetOrDefault("secret", "");
                }
            }
        }

        public class Subscription
        {
            private readonly string m_ID;
            private readonly string m_Type;
            private readonly string m_Version;
            private readonly string m_Status;
            private readonly int m_Cost;
            private readonly string m_CreatedAt;
            private readonly Transport m_Transport;
            private readonly Dictionary<string, string> m_Conditions = new();

            public string ID => m_ID;
            public string Type => m_Type;
            public string Version => m_Version;
            public string Status => m_Status;
            public int Cost => m_Cost;
            public string CreatedAt => m_CreatedAt;
            public Transport Transport => m_Transport;

            public Subscription(Json obj)
            {
                m_ID = obj.Get<string>("id")!;
                m_Type = obj.Get<string>("type")!;
                m_Version = obj.Get<string>("version")!;
                m_Status = obj.Get<string>("status")!;
                m_Cost = obj.Get<int>("cost")!;
                m_CreatedAt = obj.Get<string>("created_at")!;
                m_Transport = new(obj.Get<Json>("transport")!);
                Json conditionObject = obj.Get<Json>("condition")!;
                foreach (var pair in conditionObject.ToDictionary<string>())
                    m_Conditions[pair.Key] = pair.Value;
            }
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

            private readonly Json m_Data;

            public EventData(Json data) { m_Data = data; }

            public User? GetUser(string user = "")
            {
                if (m_Data.TryGet(string.Format("{0}user_id", user), out string? id) &&
                    m_Data.TryGet(string.Format("{0}user_login", user), out string? login) &&
                    m_Data.TryGet(string.Format("{0}user_name", user), out string? name))
                    return new(id!, login!, name!);
                return null;
            }

            public T GetOrDefault<T>(string key, T defaultValue) => m_Data.GetOrDefault(key, defaultValue);

            public bool TryGet<T>(string key, out T? ret) => m_Data.TryGet(key, out ret);
        }

        private readonly Settings.Data m_Settings;
        private readonly Quicksand.Web.WebSocket.Client m_Websocket;
        private OAuthToken? m_Token;
        private string m_ChannelID = "";
        private readonly HashSet<string> m_TreatedMessage = new();

        public EventSub(Settings.Data settings)
        {
            m_Settings = settings;
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

        private void RegisterSubscription(string sessionID, string subscriptionName, int subscriptionVersion, string condition = "broadcaster_user_id")
        {
            Json transportJson = new();
            transportJson.Set("method", "websocket");
            transportJson.Set("session_id", sessionID);
            Json conditionJson = new();
            conditionJson.Set(condition, m_ChannelID);
            Json message = new();
            message.Set("type", subscriptionName);
            message.Set("version", subscriptionVersion);
            message.Set("condition", conditionJson);
            message.Set("transport", transportJson);
            ManagedRequest request = new PostRequest("https://api.twitch.tv/helix/eventsub/subscriptions", message.ToNetworkString())
                .AddHeaderField("Content-Type", "application/json; charset=utf-8")
                .AddHeaderField("Authorization", string.Format("Bearer {0}", m_Token!.Token))
                .AddHeaderField("Client-Id", m_Token!.ClientID);
            Response? response = request.Send();
            if (response != null && response.StatusCode == 401)
            {
                m_Token.Refresh();
                request.Reset();
                response = request.Send();
            }
            if (response != null)
            {
                if (response.StatusCode == 202)
                    Logger.Log("EventSub", string.Format("<= Listening to {0}", subscriptionName));
                else
                    Logger.Log("EventSub", string.Format("<= Error when listening to {0}: {1}", subscriptionName, response.Body));
            }
        }

        private void HandleWelcome(Json payload)
        {
            Logger.Log("EventSub", "<= Welcome");
            if (payload.TryGet("session", out Json? sessionObj))
            {
                if (sessionObj!.TryGet("id", out string? sessionID))
                {
                    RegisterSubscription(sessionID!, "channel.follow", 1);
                    RegisterSubscription(sessionID!, "channel.subscribe", 1);
                    RegisterSubscription(sessionID!, "channel.subscription.gift", 1);
                    RegisterSubscription(sessionID!, "channel.raid", 1, "to_broadcaster_user_id");
                    RegisterSubscription(sessionID!, "channel.raid", 1, "from_broadcaster_user_id");
                    RegisterSubscription(sessionID!, "channel.channel_points_custom_reward_redemption.add", 1);
                    RegisterSubscription(sessionID!, "stream.online", 1);
                    RegisterSubscription(sessionID!, "stream.offline", 1);
                }
            }
        }

        private void HandleFollow(EventData data)
        {
            EventData.User? follower = data.GetUser();
            if (follower != null)
                CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs(follower.Name, new("", ""), 0, false, -1, -1, -1));
        }

        private void HandleSub(EventData data)
        {
            if (m_Settings.Get("twitch", "sub_mode") == "claimed")
                return;
            int followTier = 0;
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
                CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs(follower.Name, new("", ""), followTier, isGift, -1, -1, -1));
            }
        }

        private void HandleSubGift(EventData data)
        {
            int followTier = 0;
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
                    CanalManager.Emit(StreamGlassCanals.FOLLOW, new FollowEventArgs((follower != null) ? follower.Name : "",
                        new("", ""), followTier, true, -1, -1, (int)nbGift!));
                }
            }
        }

        private void HandleReward(EventData data)
        {
            //TODO
        }

        private void HandleRaid(EventData data)
        {
            EventData.User? from = data.GetUser("from_");
            EventData.User? to = data.GetUser("to_");
            if (from != null && to != null && data.TryGet("viewers", out int? viewers))
                CanalManager.Emit(StreamGlassCanals.RAID, new RaidEventArgs(from.ID, from.Name, to.ID, to.Name, (int)viewers!));
        }

        private void HandleStreamStart()
        {
            CanalManager.Emit(StreamGlassCanals.STREAM_START);
        }

        private void HandleStreamStop()
        {
            CanalManager.Emit(StreamGlassCanals.STREAM_STOP);
        }

        private void HandleNotification(Metadata metadata, Json payload, string message)
        {
            Logger.Log("EventSub", string.Format("<= {0}", metadata.Subscription));
            if (payload.TryGet("subscription", out Json? subscriptionObj) &&
                payload.TryGet("event", out Json? eventObj))
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
                        case "channel.raid": HandleRaid(eventData); break;
                        case "channel.channel_points_custom_reward_redemption.add": HandleReward(eventData); break;
                        case "stream.online": HandleStreamStart(); break;
                        case "stream.offline": HandleStreamStop(); break;
                        default: Logger.Log("EventSub", string.Format("<= {0}", message)); break;
                    }
                }
                catch
                {
                    Logger.Log("EventSub", string.Format("<= Missformated subscription in message: {0}", message));
                }
            }
        }

        protected override void OnWebSocketMessage(int clientID, string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            try
            {
                Json eventMessage = new(message);
                if (eventMessage.TryGet("metadata", out Json? metadataObj) &&
                    eventMessage.TryGet("payload", out Json? payload))
                {
                    try
                    {
                        Metadata metadata = new(metadataObj!);
                        if (!m_TreatedMessage.Contains(metadata.ID))
                        {
                            m_TreatedMessage.Add(metadata.ID);
                            switch (metadata.Type)
                            {
                                case "session_welcome": HandleWelcome(payload); break;
                                case "session_keepalive": break;
                                case "notification": HandleNotification(metadata, payload, message.Trim()); break;
                                case "session_reconnect": Reconnect(); break;
                                case "revocation": break;
                                default: Logger.Log("EventSub", string.Format("<= {0}", message.Trim())); break;
                            }
                        }
                    } catch
                    {
                        Logger.Log("EventSub", string.Format("<= Missformated metadata in message: {0}", message.Trim()));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("EventSub", string.Format("[Error]=> Json Exception: {0}", e));
                return;
            }
        }

        protected override void OnClientDisconnect(int clientID)
        {
            Logger.Log("EventSub", "<= Disconnected");
        }

        protected override void OnWebSocketFrame(int clientID, Frame frame)
        {
            Logger.Log("EventSub", string.Format("<=[WS] {0}", frame.ToString().Trim()));
        }

        protected override void OnWebSocketFrameSent(int clientID, Frame frame)
        {
            Logger.Log("EventSub", string.Format("[WS]=> {0}", frame.ToString().Trim()));
        }
    }
}
