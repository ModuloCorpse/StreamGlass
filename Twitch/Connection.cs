using CorpseLib;
using CorpseLib.Ini;
using CorpseLib.Web.OAuth;
using StreamGlass.Connections;
using StreamGlass.Controls;
using StreamGlass.Events;
using StreamGlass.Profile;
using StreamGlass.Settings;
using StreamGlass.Stat;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class Connection : AStreamConnection
    {
        private readonly RecurringAction m_GetViewerCount = new(500);
        private TwitchChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly TwitchAuthenticator m_Authenticator;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private TwitchHandler m_TwitchHandler = null;
        private TwitchPubSub m_CorpsePubSub = null;
        private TwitchEventSub m_EventSub = null;
        private TwitchChat m_Chat = null;
        private TwitchAPI m_API = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private RefreshToken? m_APIToken = null;
        private RefreshToken? m_IRCToken = null;

        public Connection(IniSection settings, StreamGlassWindow form): base(settings, form)
        {
            TwitchAPI.StartLogging();
            TwitchPubSub.StartLogging();
            TwitchEventSub.StartLogging();
            TwitchChat.StartLogging();
            m_Authenticator = new(settings.Get("public_key"), settings.Get("secret_key"));
            m_Authenticator.SetPageContent("<!DOCTYPE html><html><head><title>StreamGlass Twitch Auth</title></head><body><p>You can close this page</p></body></html>");
            m_GetViewerCount.OnUpdate += UpdateViewerCount;
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
            StreamGlassCanals.CHAT_JOINED.Register(OnJoinedChannel);
            StreamGlassCanals.USER_JOINED.Register(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Register(SetStreamInfo);
            StreamGlassCanals.BAN.Register(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Register(AllowMessage);

            StreamGlassContext.RegisterFunction("Game", (string[] variables) => {
                var channelInfo = m_API.GetChannelInfo(variables[0]);
                if (channelInfo != null)
                    return channelInfo.GameName;
                return variables[0];
            });
            StreamGlassContext.RegisterFunction("DisplayName", (string[] variables) => {
                var userInfo = m_API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.DisplayName;
                return variables[0];
            });
            StreamGlassContext.RegisterFunction("Channel", (string[] variables) => {
                var userInfo = m_API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.Name;
                return variables[0];
            });
        }

        protected override void AfterConnect()
        {
            m_GetViewerCount.Start();
        }

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
            m_TwitchHandler = new TwitchHandler(Settings, m_API);
            m_API.LoadGlobalEmoteSet();
            TwitchUser? creator = m_API.GetUserInfoFromLogin("chaporon_");
            if (creator != null)
                m_API.LoadChannelEmoteSet(creator);

            TwitchUser? userInfoOfToken = m_API.GetUserInfoOfToken(m_IRCToken);
            if (userInfoOfToken != null)
                m_API.LoadEmoteSetFromFollowedChannel(userInfoOfToken);
            TwitchUser selfUserInfo = m_API.GetSelfUserInfo();
            if (!string.IsNullOrEmpty(selfUserInfo.Name))
            {
                m_TwitchHandler.SetIRCChannel(selfUserInfo.Name);
                m_Chat = TwitchChat.NewConnection(m_API, selfUserInfo.Name, "StreamGlass", m_IRCToken!, m_TwitchHandler);
            }
            return true;
        }

        protected override bool OnReconnect()
        {
            m_Chat.Reconnect();
            return true;
        }

        protected override void BeforeDisconnect()
        {
            m_GetViewerCount.Stop();
        }

        protected override void Unauthenticate()
        {
            m_CorpsePubSub?.Disconnect();
            m_EventSub?.Disconnect();
            m_Chat?.Disconnect();
        }

        protected override void Clean()
        {
            ResetStreamInfo();
        }

        protected override void AfterDisconnect()
        {
            StreamGlassCanals.CHAT_JOINED.Unregister(OnJoinedChannel);
            StreamGlassCanals.USER_JOINED.Unregister(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Unregister(SetStreamInfo);
            StreamGlassCanals.BAN.Unregister(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Unregister(AllowMessage);
            StreamGlassContext.UnregisterFunction("Game");
            StreamGlassContext.UnregisterFunction("DisplayName");
            StreamGlassContext.UnregisterFunction("Channel");
        }

        public override TabItemContent[] GetSettings() => new TabItemContent[] { new TwitchSettingsItem(Settings, this) };

        private void OnJoinedChannel(string? channel)
        {
            if (channel == null)
                return;
            ResetStreamInfo();
            m_OriginalBroadcasterChannelInfo = m_API.GetChannelInfo(channel);
            Form.JoinChannel();
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                m_EventSub = TwitchEventSub.NewConnection(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_APIToken!, m_TwitchHandler);
                m_CorpsePubSub = TwitchPubSub.NewConnection(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_API, m_APIToken!, m_TwitchHandler);
            }
        }

        protected override void OnUpdate(long deltaTime) { }

        private void OnUserJoinedChannel(TwitchUser? user)
        {
            if (user == null)
                return;
            m_API.LoadEmoteSetFromFollowedChannel(user);
        }

        public override void SendMessage(string message) => m_Chat.SendMessage(message);

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

        public override CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info, m_API);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        protected override void OnTest() { }

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

        private void UpdateViewerCount(object? sender, System.EventArgs e)
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                m_TwitchHandler.UpdateViewerCountOf(m_OriginalBroadcasterChannelInfo.Broadcaster);
        }
    }
}
