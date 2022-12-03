using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.StreamChat;
using StreamGlass.UI;
using ChatClient = StreamGlass.Twitch.IRC.ChatClient;

namespace StreamGlass.Twitch
{
    public class Bot : IStreamChat, IConnection
    {
        private bool m_IsConnected = false;
        private string m_Channel = "";
        private readonly Settings.Data m_Settings;
        private readonly ChatClient m_Client;
        private ChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly Authenticator m_Authenticator;
        private readonly StreamGlassWindow m_Form;
        private readonly EventSub m_PubSub;

        public Bot(Server webServer, Settings.Data settings, StreamGlassWindow form)
        {
            m_Settings = settings;
            m_Form = form;
            m_Authenticator = new(webServer, settings);
            //This section will create the settings variables ONLY if they where not loaded
            m_Settings.Create("twitch", "auto_connect", "false");
            m_Settings.Create("twitch", "browser", "");
            m_Settings.Create("twitch", "channel", "");
            m_Settings.Create("twitch", "public_key", "");
            m_Settings.Create("twitch", "secret_key", "");
            m_Settings.Create("twitch", "sub_mode", "all");

            m_Client = new(m_Settings);
            m_PubSub = new(m_Settings);

            if (m_Settings.Get("twitch", "auto_connect") == "true")
                Connect();
        }

        private void Register()
        {
            CanalManager.Register(StreamGlassCanals.CHAT_CONNECTED, OnConnected);
            CanalManager.Register<string>(StreamGlassCanals.CHAT_JOINED, OnJoinedChannel);
            CanalManager.Register<string>(StreamGlassCanals.USER_JOINED, OnUserJoinedChannel);
            CanalManager.Register<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);

            ChatCommand.AddFunction("Game", (string[] variables) => {
                var channelInfo = API.GetChannelInfoFromLogin(variables[0]);
                if (channelInfo != null)
                    return channelInfo.GameName;
                return variables[0];
            });
            ChatCommand.AddFunction("DisplayName", (string[] variables) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.DisplayName;
                return variables[0];
            });
            ChatCommand.AddFunction("Channel", (string[] variables) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.Login;
                return variables[0];
            });
        }

        private void Unregister()
        {
            CanalManager.Unregister(StreamGlassCanals.CHAT_CONNECTED, OnConnected);
            CanalManager.Unregister<string>(StreamGlassCanals.CHAT_JOINED, OnJoinedChannel);
            CanalManager.Unregister<string>(StreamGlassCanals.USER_JOINED, OnUserJoinedChannel);
            CanalManager.Unregister<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
            ChatCommand.RemoveFunction("Game");
            ChatCommand.RemoveFunction("DisplayName");
            ChatCommand.RemoveFunction("Channel");
        }

        public void Connect()
        {
            if (!m_IsConnected)
            {
                Register();
                OAuthToken? apiToken = m_Authenticator.Authenticate();
                if (apiToken != null)
                {
                    m_IsConnected = true;
                    m_PubSub.SetToken(apiToken);
                    API.Authenticate(apiToken);
                    API.LoadGlobalEmoteSet();
                    API.LoadChannelEmoteSetFromLogin("chaporon_");
                    OAuthToken? ircToken;
                    string browser = m_Settings.Get("twitch", "browser");
                    if (string.IsNullOrWhiteSpace(browser))
                        ircToken = apiToken;
                    else
                        ircToken = m_Authenticator.Authenticate(browser);
                    if (ircToken != null)
                    {
                        UserInfo? userInfoOfToken = API.GetUserInfoOfToken(ircToken);
                        m_Client.SetSelfUserInfo(userInfoOfToken);
                        if (userInfoOfToken != null)
                            API.LoadEmoteSetFromFollowedChannelOfID(userInfoOfToken.ID);
                        m_Client.Init("StreamGlass", ircToken);
                    }
                }
            }
        }

        public void Disconnect()
        {
            m_PubSub.Disconnect();
            m_Client.Disconnect();
            ResetStreamInfo();
            Unregister();
        }

        public Settings.TabItem GetSettings() => new TwitchSettingsItem(m_Settings, this);

        private void OnConnected(int _)
        {
            m_Client.Join(m_Settings.Get("twitch", "channel"));
        }

        private void OnJoinedChannel(int _, string channel)
        {
            if (m_Channel != channel)
            {
                m_Channel = channel;
                API.LoadChannelEmoteSetFromLogin(channel[1..]);
                ResetStreamInfo();
                m_OriginalBroadcasterChannelInfo = API.GetChannelInfoFromLogin(channel[1..]);
                m_Form.JoinChannel(channel);
                m_Client.SendMessage(channel, "Hello World! Je suis un bot connecté via StreamGlass!");
                if (m_OriginalBroadcasterChannelInfo != null)
                {
                    m_PubSub.Connect(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
                }
            }
        }

        public void Update(long _) {}

        private void OnUserJoinedChannel(int _, string login)
        {
            API.LoadEmoteSetFromFollowedChannelOfLogin(login);
        }

        public void SendMessage(string channel, string message) => m_Client.SendMessage(channel, message);

        public string GetEmoteURL(string emoteID, BrushPaletteManager palette)
        {
            return API.GetEmoteURL(emoteID, palette.GetPaletteType());
        }

        private void ResetStreamInfo()
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
        }

        private void SetStreamInfo(int _, UpdateStreamInfoArgs args)
        {
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                if (string.IsNullOrWhiteSpace(args.Title) && string.IsNullOrWhiteSpace(args.Game) && string.IsNullOrWhiteSpace(args.Language))
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
                else
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, args.Title, args.Game, args.Language);
            }
        }

        /*private void Client_OnNewSubscriber(object? sender, OnNewSubscriberArgs e)
        {
            int tier = 1;
            bool isPrime = false;
            switch (e.Subscriber.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime: isPrime = true; break;
                case SubscriptionPlan.Tier2: tier = 2; break;
                case SubscriptionPlan.Tier3: tier = 3; break;
            }
        }*/
    }
}
