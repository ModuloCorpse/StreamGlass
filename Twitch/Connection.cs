using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.Controls;
using ChatClient = StreamGlass.Twitch.IRC.ChatClient;
using StreamGlass.Connections;
using StreamGlass.Settings;
using StreamGlass.Events;
using CorpseLib.Ini;
using CorpseLib.Web.OAuth;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class Connection : AStreamConnection
    {
        private readonly ChatClient m_Client;
        private readonly EventSub m_EventSub;
        private readonly PubSub m_PubSub;
        private ChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly Authenticator m_Authenticator;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private API m_API = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private RefreshToken? m_APIToken = null;
        private RefreshToken? m_IRCToken = null;
        private string m_Channel = "";

        public Connection(Server webServer, IniSection settings, StreamGlassWindow form): base(settings, form)
        {
            API.StartLogging();
            m_Authenticator = new(webServer, settings);
            m_Client = new(settings);
            m_EventSub = new(settings);
            m_PubSub = new();
        }

        protected override void InitSettings()
        {
            CreateSetting("browser", "");
            CreateSetting("public_key", "");
            CreateSetting("secret_key", "");
            CreateSetting("sub_mode", "all");
        }

        protected override void LoadSettings() { }

        protected override void BeforeConnect()
        {
            StreamGlassCanals.CHAT_CONNECTED.Register(OnConnected);
            StreamGlassCanals.CHAT_JOINED.Register(OnJoinedChannel);
            StreamGlassCanals.USER_JOINED.Register(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Register(SetStreamInfo);
            StreamGlassCanals.BAN.Register(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Register(AllowMessage);

            ChatCommand.AddFunction("Game", (string[] variables) => {
                var channelInfo = m_API.GetChannelInfo(variables[0]);
                if (channelInfo != null)
                    return channelInfo.GameName;
                return variables[0];
            });
            ChatCommand.AddFunction("DisplayName", (string[] variables) => {
                var userInfo = m_API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.DisplayName;
                return variables[0];
            });
            ChatCommand.AddFunction("Channel", (string[] variables) => {
                var userInfo = m_API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.Name;
                return variables[0];
            });
        }

        protected override void AfterConnect() { }

        protected override bool Authenticate()
        {
            m_APIToken = m_Authenticator.Authenticate();
            string browser = GetSetting("browser");
            if (string.IsNullOrWhiteSpace(browser))
                m_IRCToken = m_APIToken;
            else
                m_IRCToken = m_Authenticator.Authenticate(browser);
            return m_APIToken != null && m_IRCToken != null;
        }

        protected override bool Init()
        {
            if (m_APIToken == null || m_IRCToken == null)
                return false;
            m_API = new(m_APIToken);
            m_Client.SetAPI(m_API);
            m_PubSub.SetAPI(m_API);
            m_EventSub.SetToken(m_APIToken);
            m_PubSub.SetToken(m_APIToken);
            m_API.LoadGlobalEmoteSet();
            User? creator = m_API.GetUserInfoFromLogin("chaporon_");
            if (creator != null)
                m_API.LoadChannelEmoteSet(creator);

            User? userInfoOfToken = m_API.GetUserInfoOfToken(m_IRCToken);
            m_Client.SetSelfUserInfo(userInfoOfToken);
            if (userInfoOfToken != null)
                m_API.LoadEmoteSetFromFollowedChannel(userInfoOfToken);
            m_Client.Init("StreamGlass", m_IRCToken);
            return true;
        }

        protected override bool OnReconnect()
        {
            m_Client.Reconnect();
            return true;
        }

        internal void ReconnectEventSub() => m_EventSub.Reconnect();

        protected override void BeforeDisconnect() { }

        protected override void Unauthenticate()
        {
            m_PubSub.Disconnect();
            m_EventSub.Disconnect();
            m_Client.Disconnect();
        }

        protected override void Clean()
        {
            ResetStreamInfo();
        }

        protected override void AfterDisconnect()
        {
            StreamGlassCanals.CHAT_CONNECTED.Unregister(OnConnected);
            StreamGlassCanals.CHAT_JOINED.Unregister(OnJoinedChannel);
            StreamGlassCanals.USER_JOINED.Unregister(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Unregister(SetStreamInfo);
            StreamGlassCanals.BAN.Unregister(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Unregister(AllowMessage);
            ChatCommand.RemoveFunction("Game");
            ChatCommand.RemoveFunction("DisplayName");
            ChatCommand.RemoveFunction("Channel");
        }

        public override TabItemContent[] GetSettings() => new TabItemContent[] { new TwitchSettingsItem(Settings, this) };

        private void OnConnected()
        {
            User selfUserInfo = m_API.GetSelfUserInfo();
            if (!string.IsNullOrEmpty(selfUserInfo.Name))
                m_Client.Join(selfUserInfo.Name);
        }

        private void OnJoinedChannel(string? channel)
        {
            if (channel == null)
                return;
            if (m_Channel != channel)
            {
                m_Channel = channel;
                ResetStreamInfo();
                m_OriginalBroadcasterChannelInfo = m_API.GetChannelInfo(channel[1..]);
                Form.JoinChannel(channel);
                if (m_OriginalBroadcasterChannelInfo != null)
                {
                    m_EventSub.Connect(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
                    m_PubSub.Connect(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
                }
            }
        }

        protected override void OnUpdate(long deltaTime)
        {
            m_PubSub.Update(deltaTime);
        }

        private void OnUserJoinedChannel(User? user)
        {
            if (user == null)
                return;
            m_API.LoadEmoteSetFromFollowedChannel(user);
        }

        public override void SendMessage(string channel, string message) => m_Client.SendMessage(channel, message);

        private void ResetStreamInfo()
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                m_API.SetChannelInfo(m_OriginalBroadcasterChannelInfo.Broadcaster, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
        }

        private void SetStreamInfo(UpdateStreamInfoArgs? arg)
        {
            if (arg == null)
                return;
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                string title = (string.IsNullOrWhiteSpace(arg.Title)) ? m_OriginalBroadcasterChannelInfo.Title : arg.Title;
                string category = (string.IsNullOrWhiteSpace(arg.Category.ID)) ? m_OriginalBroadcasterChannelInfo.GameID : arg.Category.ID;
                string language = (string.IsNullOrWhiteSpace(arg.Language)) ? m_OriginalBroadcasterChannelInfo.BroadcasterLanguage : arg.Language;
                m_API.SetChannelInfo(m_OriginalBroadcasterChannelInfo.Broadcaster, title, category, language);
            }
        }

        public override Profile.CategoryInfo? SearchCategoryInfo(Window parent, Profile.CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info, m_API);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        protected override void OnTest()
        {
            string raidEvent = "{\"metadata\":{\"message_id\":\"Eksw0vF_C8pr18N2QurlWf10SbjYxwm_S3i5iejL3F0=\",\"message_type\":\"notification\",\"message_timestamp\":\"2023-01-21T23:05:09.22883373Z\",\"subscription_type\":\"channel.raid\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"5f67ff22-76ad-47aa-84d1-061757c0a8a2\",\"status\":\"enabled\",\"type\":\"channel.raid\",\"version\":\"1\",\"condition\":{\"from_broadcaster_user_id\":\"52792239\",\"to_broadcaster_user_id\":\"\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AQoQMvTjh3BiR_SeA_WDbO29khAB\"},\"created_at\":\"2023-01-21T21:15:16.482046894Z\",\"cost\":0},\"event\":{\"from_broadcaster_user_id\":\"52792239\",\"from_broadcaster_user_login\":\"chaporon_\",\"from_broadcaster_user_name\":\"ChapORon_\",\"to_broadcaster_user_id\":\"164831380\",\"to_broadcaster_user_login\":\"fxt_volv\",\"to_broadcaster_user_name\":\"FxT_VolV\",\"viewers\":2}}}";
            m_EventSub.OnWebSocketMessage(0, raidEvent);

            string heldMessage = "{\"type\":\"MESSAGE\",\"data\":{\"topic\":\"automod-queue.52792239.52792239\",\"message\":\"{\\\"type\\\":\\\"automod_caught_message\\\",\\\"data\\\":{\\\"content_classification\\\":{\\\"category\\\":\\\"homophobia\\\",\\\"level\\\":1},\\\"message\\\":{\\\"content\\\":{\\\"text\\\":\\\"sale gay\\\",\\\"fragments\\\":[{\\\"text\\\":\\\"sale gay\\\",\\\"automod\\\":{\\\"topics\\\":{\\\"identity\\\":7}}}]},\\\"id\\\":\\\"6b650b9d-a719-4760-976a-fbb74a5b3313\\\",\\\"sender\\\":{\\\"user_id\\\":\\\"750309830\\\",\\\"login\\\":\\\"crashtestroadto8c\\\",\\\"display_name\\\":\\\"crashtestroadto8c\\\"},\\\"sent_at\\\":\\\"2023-02-01T15:50:49.103887653Z\\\"},\\\"reason_code\\\":\\\"\\\",\\\"resolver_id\\\":\\\"\\\",\\\"resolver_login\\\":\\\"\\\",\\\"status\\\":\\\"PENDING\\\"}}\"}}\r\n";
            m_PubSub.OnWebSocketMessage(0, heldMessage);
        }

        private void BanUser(BanEventArgs? arg)
        {
            if (arg == null)
                return;
            m_API.BanUser(arg.User, arg.Reason, arg.Delay);
        }

        private void AllowMessage(MessageAllowedEventArgs? arg)
        {
            if (arg == null)
                return;
            m_API.ManageHeldMessage(arg.MessageID, arg.IsAllowed);
        }
    }
}
