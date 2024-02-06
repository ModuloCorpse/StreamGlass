using CorpseLib;
using CorpseLib.Ini;
using CorpseLib.Network;
using CorpseLib.Serialize;
using CorpseLib.Web.WebSocket;
using StreamGlass.Core;
using StreamGlass.Core.Connections;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Events;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Settings;
using System.Text;
using TwitchCorpse;
using static TwitchCorpse.TwitchEventSub;

namespace StreamGlass.Twitch
{
    public class Connection : AStreamConnection
    {
        private static readonly string AUTHENTICATOR_PAGE_CONTENT = "<!DOCTYPE html><html><head><title>StreamGlass Twitch Auth</title></head><body><p>You can close this page</p></body></html>";

        private readonly ProfileManager m_ProfileManager;
        private readonly ConnectionManager m_ConnectionManager;
        private readonly RecurringAction m_GetViewerCount = new(500);
        private TwitchChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly TwitchAuthenticator m_Authenticator;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private TwitchHandler m_TwitchHandler = null;
        private TwitchPubSub m_PubSub = null;
        private TwitchEventSub m_EventSub = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private readonly TwitchAPI m_API = new();
        private TwitchAPI m_IRCAPI = new();
        private readonly SubscriptionType[] m_SubscriptionTypes = [
            SubscriptionType.ChannelFollow,
            SubscriptionType.ChannelSubscribe,
            SubscriptionType.ChannelSubscriptionGift,
            SubscriptionType.ChannelRaid,
            SubscriptionType.ChannelChannelPointsCustomRewardRedemptionAdd,
            SubscriptionType.StreamOnline,
            SubscriptionType.StreamOffline,
            SubscriptionType.ChannelShoutoutCreate,
            SubscriptionType.ChannelShoutoutReceive,
            SubscriptionType.ChannelChatClear,
            SubscriptionType.ChannelChatClearUserMessages,
            SubscriptionType.ChannelChatMessage,
            SubscriptionType.ChannelChatMessageDelete,
            SubscriptionType.ChannelChatNotification
        ];

        public Connection(ProfileManager profileManager, ConnectionManager connectionManager, IniSection settings): base(settings)
        {
            m_ProfileManager = profileManager;
            m_ConnectionManager = connectionManager;

            TwitchAPI.StartLogging();
            TwitchPubSub.StartLogging();
            TwitchEventSub.StartLogging();
            TwitchChat.StartLogging();
            m_Authenticator = new(settings.Get("public_key"), settings.Get("secret_key"));
            m_Authenticator.SetPageContent("<!DOCTYPE html><html><head><title>StreamGlass Twitch Auth</title></head><body><p>You can close this page</p></body></html>");
            m_GetViewerCount.OnUpdate += UpdateViewerCount;
        }

        protected override void LoadSettings() { }

        protected override void BeforeConnect()
        {
            StreamGlassCanals.USER_JOINED.Register(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Register(SetStreamInfo);
            StreamGlassCanals.BAN.Register(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Register(AllowMessage);
            StreamGlassCanals.START_ADS.Register(OnStartAds);

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
            m_API.Authenticate(Settings.Get("public_key"), Settings.Get("secret_key"), 3000, AUTHENTICATOR_PAGE_CONTENT);
            string browser = GetSetting("browser");
            if (string.IsNullOrWhiteSpace(browser))
                m_IRCAPI = m_API;
            else
                m_IRCAPI.AuthenticateWithBrowser(Settings.Get("public_key"), Settings.Get("secret_key"), 3000, AUTHENTICATOR_PAGE_CONTENT, browser);
            return m_API.IsAuthenticated && m_IRCAPI.IsAuthenticated;
        }

        protected override bool Init()
        {
            if (!m_API.IsAuthenticated || !m_IRCAPI.IsAuthenticated)
                return false;
            m_TwitchHandler = new TwitchHandler(Settings, m_API);
            m_API.SetHandler(m_TwitchHandler);
            m_API.LoadGlobalEmoteSet();
            m_API.LoadGlobalChatBadges();
            TwitchUser? creator = m_API.GetUserInfoFromLogin("chaporon_");
            if (creator != null)
                m_API.LoadChannelEmoteSet(creator);

            TwitchUser? userInfoOfToken = m_IRCAPI.GetSelfUserInfo();
            if (userInfoOfToken != null)
                m_API.LoadEmoteSetFromFollowedChannel(userInfoOfToken);
            TwitchUser selfUserInfo = m_API.GetSelfUserInfo();
            m_API.LoadChannelChatBadges(selfUserInfo);
            if (!string.IsNullOrEmpty(selfUserInfo.Name))
                m_TwitchHandler.SetIRCChannel(selfUserInfo.Name);


            ResetStreamInfo();
            m_OriginalBroadcasterChannelInfo = m_API.GetChannelInfo(selfUserInfo);

            m_ProfileManager.UpdateStreamInfo();

            if (m_OriginalBroadcasterChannelInfo != null)
            {
                TwitchEventSub? eventSub = m_API.EventSubConnection(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_SubscriptionTypes);
                if (eventSub != null)
                {
                    m_EventSub = eventSub;
                    m_EventSub.SetMonitor(new DebugLogMonitor(TwitchEventSub.LOGGER));
                    m_EventSub.OnWelcome += (object? sender, EventArgs e) =>
                    {
                        if (Settings.Get("do_welcome") == "true")
                            m_ConnectionManager.SendMessage(Settings.Get("welcome_message"));
                    };
                }
                TwitchPubSub? pubSub = m_API.PubSubConnection(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
                if (pubSub != null)
                {
                    m_PubSub = pubSub;
                    m_PubSub.SetMonitor(new DebugLogMonitor(TwitchPubSub.PUBSUB));
                }
            }

            return true;
        }



        protected override bool OnReconnect() => true;

        protected override void BeforeDisconnect()
        {
            m_GetViewerCount.Stop();
        }

        protected override void Unauthenticate()
        {
            m_PubSub?.Disconnect();
            m_EventSub?.Disconnect();
        }

        protected override void Clean()
        {
            ResetStreamInfo();
        }

        protected override void AfterDisconnect()
        {
            StreamGlassCanals.USER_JOINED.Unregister(OnUserJoinedChannel);
            StreamGlassCanals.UPDATE_STREAM_INFO.Unregister(SetStreamInfo);
            StreamGlassCanals.BAN.Unregister(BanUser);
            StreamGlassCanals.ALLOW_MESSAGE.Unregister(AllowMessage);
            StreamGlassContext.UnregisterFunction("Game");
            StreamGlassContext.UnregisterFunction("DisplayName");
            StreamGlassContext.UnregisterFunction("Channel");
        }

        public override TabItemContent[] GetSettings() => new TabItemContent[] { new TwitchSettingsItem(Settings, this) };

        protected override void OnUpdate(long deltaTime) { }

        private void OnUserJoinedChannel(TwitchUser? user)
        {
            if (user == null)
                return;
            m_API.LoadEmoteSetFromFollowedChannel(user);
        }

        public override void SendMessage(string message)
        {
            m_IRCAPI.PostMessage(m_API.GetSelfUserInfo(), message);
        }

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

        private void OnStartAds(uint duration) => m_API.StartCommercial(duration);

        protected override void OnTest()
        {
            if (!IsConnected)
                return;
            TwitchUser m_StreamGlass = m_IRCAPI.GetSelfUserInfo();
            TwitchUser m_Self = m_API.GetSelfUserInfo();
            StreamGlassContext.LOGGER.Log("Testing Twitch");
            StreamGlassContext.LOGGER.Log("Testing follow");
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 0, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 1");
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 1, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 2");
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 2, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 3");
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 3, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing prime sub");
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 4, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing gift sub tier 1");
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(m_Self, m_StreamGlass, new("Il aime le Pop-Corn"), 1, 69, 42, -1));
            StreamGlassContext.LOGGER.Log("Testing bits donation");
            StreamGlassCanals.DONATION.Emit(new(m_StreamGlass, 666, "bits", new("J'aime le Pop-Corn")));
            StreamGlassContext.LOGGER.Log("Testing incomming raid");
            StreamGlassCanals.RAID.Emit(new(m_StreamGlass, m_Self, 40, true));
            StreamGlassContext.LOGGER.Log("Testing reward");
            StreamGlassCanals.REWARD.Emit(new(m_StreamGlass, "Chante", "J'aime le Pop-Corn"));
            StreamGlassContext.LOGGER.Log("Testing shoutout");
            StreamGlassCanals.SHOUTOUT.Emit(new(m_StreamGlass, m_Self));
            StreamGlassContext.LOGGER.Log("Testing incomming shoutout");
            StreamGlassCanals.BEING_SHOUTOUT.Emit(m_StreamGlass);
            StreamGlassContext.LOGGER.Log("Twitch tested");

            BytesWriter bytesWriter = m_EventSub.CreateBytesWriter();
            bytesWriter.Write(new Frame(true, 1, Encoding.UTF8.GetBytes("{\"metadata\":{\"message_id\":\"8tFzUmxgpeIhaX0wzBUKeA2KXpeWbc3gAQk7IUzaYBg=\",\"message_type\":\"notification\",\"message_timestamp\":\"2024-01-31T20:46:26.859382297Z\",\"subscription_type\":\"channel.chat.message\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"98860814-754a-4e0c-980b-510640ea008d\",\"status\":\"enabled\",\"type\":\"channel.chat.message\",\"version\":\"1\",\"condition\":{\"broadcaster_user_id\":\"52792239\",\"user_id\":\"52792239\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AgoQ0dkFoHiWS9mnjILZYMMazxIGY2VsbC1j\"},\"created_at\":\"2024-01-31T19:20:00.500247071Z\",\"cost\":0},\"event\":{\"broadcaster_user_id\":\"52792239\",\"broadcaster_user_login\":\"chaporon_\",\"broadcaster_user_name\":\"ChapORon_\",\"chatter_user_id\":\"737196630\",\"chatter_user_login\":\"dragshur\",\"chatter_user_name\":\"Dragshur\",\"message_id\":\"28f5dbe3-3543-4245-9497-e1e381f92238\",\"message\":{\"text\":\"Cheer10 GG\",\"fragments\":[{\"type\":\"cheermote\",\"text\":\"Cheer10\",\"cheermote\":{\"prefix\":\"cheer\",\"bits\":10,\"tier\":1},\"emote\":null,\"mention\":null},{\"type\":\"text\",\"text\":\" GG\",\"cheermote\":null,\"emote\":null,\"mention\":null}]},\"color\":\"#1E90FF\",\"badges\":[{\"set_id\":\"moderator\",\"id\":\"1\",\"info\":\"\"},{\"set_id\":\"founder\",\"id\":\"0\",\"info\":\"3\"},{\"set_id\":\"hype-train\",\"id\":\"1\",\"info\":\"\"}],\"message_type\":\"text\",\"cheer\":{\"bits\":10},\"reply\":null,\"channel_points_custom_reward_id\":null}}}")));
            m_EventSub.TestRead(bytesWriter);
        }
    }
}
