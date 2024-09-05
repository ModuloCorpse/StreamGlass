using CorpseLib;
using CorpseLib.Network;
using CorpseLib.Placeholder;
using StreamGlass.Core;
using StreamGlass.Core.Profile;
using StreamGlass.Twitch.Events;
using TwitchCorpse;
using TwitchCorpse.API;
using static TwitchCorpse.TwitchEventSub;

namespace StreamGlass.Twitch
{
    public class Core
    {
        private static readonly string AUTHENTICATOR_PAGE_CONTENT = "<!DOCTYPE html><html><head><title>StreamGlass Twitch Auth</title></head><body><p>You can close this page</p></body></html>";

        private readonly RecurringAction m_GetViewerCount = new(500);
        private TwitchChannelInfo? m_OriginalBroadcasterChannelInfo = null;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private Settings m_Settings = null;
        private Handler m_TwitchHandler = null;
        private TwitchEventSub m_EventSub = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private TwitchAPI? m_API;
        private TwitchAPI? m_IRCAPI;
        private bool m_IsConnected = false;
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
            SubscriptionType.ChannelChatNotification,
            SubscriptionType.AutomodMessageHeld,
            SubscriptionType.AutomodMessageUpdate
        ];

        public Core()
        {
            TwitchAPI.StartLogging();
            TwitchEventSub.StartLogging();
            m_GetViewerCount.OnUpdate += UpdateViewerCount;
        }

        public bool IsSelf(TwitchUser user) => user.ID == m_API!.GetSelfUserInfo().ID;
        public bool IsIRCSelf(TwitchUser user) => user.ID == m_IRCAPI!.GetSelfUserInfo().ID;

        internal void SetSettings(Settings settings) => m_Settings = settings;

        private bool Authenticate(string publicKey, string privateKey, string browser)
        {
            m_API!.AuthenticateFromTokenFile("twitch_api_token");
            m_IRCAPI!.AuthenticateFromTokenFile("twitch_irc_api_token");
            if (!m_API.IsAuthenticated)
                m_API.AuthenticateWithBrowser();
            if (!m_IRCAPI!.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(browser))
                    m_IRCAPI = m_API;
                else
                    m_IRCAPI!.AuthenticateWithBrowser(browser);
            }
            return m_API.IsAuthenticated && m_IRCAPI.IsAuthenticated;
        }

        private bool Init()
        {
            if (m_API == null || !m_API.IsAuthenticated || m_IRCAPI == null || !m_IRCAPI.IsAuthenticated)
                return false;
            m_TwitchHandler = new Handler(this, m_Settings, m_API);
            m_API.SetHandler(m_TwitchHandler);
            m_API.ResetCache();

            TwitchUser selfUserInfo = m_API.GetSelfUserInfo();
            string selfUserInfoName = selfUserInfo.Name;
            if (!string.IsNullOrEmpty(selfUserInfoName))
            {
                m_TwitchHandler.SetIRCChannel(selfUserInfoName);
                StreamGlassContext.RegisterVariable("Self", selfUserInfoName);
            }

            ResetStreamInfo();
            m_OriginalBroadcasterChannelInfo = m_API.GetChannelInfo(selfUserInfo);

            if (m_OriginalBroadcasterChannelInfo != null)
            {
                TwitchEventSub? eventSub = m_API.EventSubConnection(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_SubscriptionTypes);
                if (eventSub != null)
                {
                    m_EventSub = eventSub;
                    m_EventSub.SetMonitor(new DebugLogMonitor(TwitchEventSub.LOGGER));
                    m_EventSub.OnWelcome += (object? sender, EventArgs e) =>
                    {
                        if (m_Settings.DoWelcome)
                            PostMessage(m_Settings.WelcomeMessage);
                    };
                }
            }

            return true;
        }

        public void OnPluginLoad()
        {
            StreamGlassContext.RegisterFunction("Title", StreamGlassPlaceholdersFunction_Title);
            StreamGlassContext.RegisterFunction("Game", StreamGlassPlaceholdersFunction_Game);
            StreamGlassContext.RegisterFunction("DisplayName", StreamGlassPlaceholdersFunction_DisplayName);
            StreamGlassContext.RegisterFunction("Channel", StreamGlassPlaceholdersFunction_Channel);
            StreamGlassContext.RegisterFunction("Avatar", StreamGlassPlaceholdersFunction_Avatar);
            StreamGlassContext.RegisterFunction("BoxArt", StreamGlassPlaceholdersFunction_BoxArt);
        }

        public void OnPluginInit()
        {
            StreamGlassCanals.Register(TwitchPlugin.Canals.STREAM_START, OnStreamStart);
            StreamGlassCanals.Register<string>(StreamGlassCanals.SEND_MESSAGE, PostMessage);
            StreamGlassCanals.Register<TwitchUser>(TwitchPlugin.Canals.USER_JOINED, OnUserJoinedChannel);
            StreamGlassCanals.Register<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
            StreamGlassCanals.Register<BanEventArgs>(TwitchPlugin.Canals.BAN, BanUser);
            StreamGlassCanals.Register<TwitchUser>(TwitchPlugin.Canals.SEND_SHOUTOUT, ShoutoutUser);
            StreamGlassCanals.Register<MessageAllowedEventArgs>(TwitchPlugin.Canals.ALLOW_MESSAGE, AllowMessage);
        }

        private void ShoutoutUser(TwitchUser? user)
        {
            if (user == null)
                return;
            m_API!.ShoutoutUser(user);
        }

        private TwitchUser? GetUserInfo(string login, Cache cache)
        {
            string cacheValueName = string.Format("{0}_twitch_user", login);
            TwitchUser? user = cache.GetCachedValue<TwitchUser>(cacheValueName);
            if (user == null)
            {
                user = m_API!.GetUserInfoFromLogin(login);
                if (user != null)
                    cache.CacheValue(cacheValueName, user);
            }
            return user;
        }

        private TwitchChannelInfo? GetChannelInfo(string login, Cache cache)
        {
            string cacheValueName = string.Format("{0}_twitch_channel_info", login);
            TwitchChannelInfo? channelInfo = cache.GetCachedValue<TwitchChannelInfo>(cacheValueName);
            if (channelInfo != null)
                return channelInfo;
            TwitchUser? user = GetUserInfo(login, cache);
            if (user != null)
            {
                channelInfo = m_API!.GetChannelInfo(user);
                if (channelInfo != null)
                    cache.CacheValue(cacheValueName, channelInfo);
            }
            return channelInfo;
        }

        private TwitchCategoryInfo? GetChannelCategoryInfo(string login, Cache cache)
        {
            string cacheValueName = string.Format("{0}_twitch_category_info", login);
            TwitchCategoryInfo? categoryInfo = cache.GetCachedValue<TwitchCategoryInfo>(cacheValueName);
            if (categoryInfo != null)
                return categoryInfo;
            TwitchChannelInfo? channelInfo = GetChannelInfo(login, cache);
            if (channelInfo != null)
            {
                categoryInfo = m_API!.GetCategoryInfo(channelInfo.GameID, channelInfo.GameName);
                if (categoryInfo != null)
                    cache.CacheValue(cacheValueName, categoryInfo);
            }
            return categoryInfo;
        }

        private string StreamGlassPlaceholdersFunction_Title(string[] variables, Cache cache)
        {
            TwitchChannelInfo? channelInfo = GetChannelInfo(variables[0], cache);
            if (channelInfo != null)
                return channelInfo.Title;
            return variables[0];
        }

        private string StreamGlassPlaceholdersFunction_Game(string[] variables, Cache cache)
        {
            TwitchChannelInfo? channelInfo = GetChannelInfo(variables[0], cache);
            if (channelInfo != null)
                return channelInfo.GameName;
            return variables[0];
        }

        private string StreamGlassPlaceholdersFunction_BoxArt(string[] variables, Cache cache)
        {
            TwitchCategoryInfo? categoryInfo = GetChannelCategoryInfo(variables[0], cache);
            if (categoryInfo != null)
                return categoryInfo.ImageURL;
            return variables[0];
        }

        private string StreamGlassPlaceholdersFunction_DisplayName(string[] variables, Cache cache)
        {
            TwitchUser? userInfo = GetUserInfo(variables[0], cache);
            if (userInfo != null)
                return userInfo.DisplayName;
            return variables[0];
        }

        private string StreamGlassPlaceholdersFunction_Channel(string[] variables, Cache cache)
        {
            TwitchUser? userInfo = GetUserInfo(variables[0], cache);
            if (userInfo != null)
                return userInfo.Name;
            return variables[0];
        }

        private string StreamGlassPlaceholdersFunction_Avatar(string[] variables, Cache cache)
        {
            TwitchUser? userInfo = GetUserInfo(variables[0], cache);
            if (userInfo != null)
                return userInfo.ProfileImageURL;
            return variables[0];
        }

        public bool Connect()
        {
            if (!m_IsConnected)
            {
                m_API = new(m_Settings.PublicKey, m_Settings.SecretKey, 3000, AUTHENTICATOR_PAGE_CONTENT);
                m_IRCAPI = new(m_Settings.PublicKey, m_Settings.SecretKey, 3000, AUTHENTICATOR_PAGE_CONTENT);
                if (!Authenticate(m_Settings.PublicKey, m_Settings.SecretKey, m_Settings.Browser))
                    return false;
                if (Init())
                {
                    m_IsConnected = true;
                    return true;
                }
            }
            return false;
        }

        public void Disconnect()
        {
            if (m_IsConnected)
            {
                m_API!.SaveAPIToken("twitch_api_token");
                m_IRCAPI!.SaveAPIToken("twitch_irc_api_token");
                m_GetViewerCount.Stop();
                ResetStreamInfo();
                m_EventSub?.Disconnect();
                m_IsConnected = false;
                StreamGlassCanals.Unregister(TwitchPlugin.Canals.STREAM_START, OnStreamStart);
                StreamGlassCanals.Unregister<string>(StreamGlassCanals.SEND_MESSAGE, PostMessage);
                StreamGlassCanals.Unregister<TwitchUser>(TwitchPlugin.Canals.USER_JOINED, OnUserJoinedChannel);
                StreamGlassCanals.Unregister<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
                StreamGlassCanals.Unregister<BanEventArgs>(TwitchPlugin.Canals.BAN, BanUser);
                StreamGlassCanals.Unregister<MessageAllowedEventArgs>(TwitchPlugin.Canals.ALLOW_MESSAGE, AllowMessage);
                StreamGlassCanals.Unregister<TwitchUser>(TwitchPlugin.Canals.SEND_SHOUTOUT, ShoutoutUser);
                StreamGlassContext.UnregisterFunction("Game");
                StreamGlassContext.UnregisterFunction("DisplayName");
                StreamGlassContext.UnregisterFunction("Channel");
                StreamGlassContext.UnregisterFunction("Avatar");
            }
        }

        private void UpdateViewerCount(object? sender, EventArgs e)
        {
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                if (m_TwitchHandler.UpdateViewerCountOf(m_OriginalBroadcasterChannelInfo.Broadcaster))
                    return;
            }
            //Stopping viewer count action as either information are missing or stream isn't started yet
            m_GetViewerCount.Stop();
        }

        private void ResetStreamInfo()
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                m_API!.SetChannelInfo(m_OriginalBroadcasterChannelInfo.Broadcaster, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
        }

        private void OnStreamStart() => m_GetViewerCount.Start();

        private void OnUserJoinedChannel(TwitchUser? _) { }

        private void SetStreamInfo(UpdateStreamInfoArgs? arg)
        {
            if (arg == null)
                return;
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                string title = (string.IsNullOrWhiteSpace(arg.Title)) ? m_OriginalBroadcasterChannelInfo.Title : arg.Title;
                string category = (string.IsNullOrWhiteSpace(arg.Category.ID)) ? m_OriginalBroadcasterChannelInfo.GameID : arg.Category.ID;
                string language = (string.IsNullOrWhiteSpace(arg.Language)) ? m_OriginalBroadcasterChannelInfo.BroadcasterLanguage : arg.Language;
                m_API!.SetChannelInfo(m_OriginalBroadcasterChannelInfo.Broadcaster, title, category, language);
            }
        }

        private void BanUser(BanEventArgs? arg)
        {
            if (arg == null)
                return;
            m_API!.BanUser(arg.User, arg.Reason, arg.Delay);
        }

        private void AllowMessage(MessageAllowedEventArgs? arg)
        {
            if (arg == null)
                return;
            m_API!.ManageHeldMessage(arg.MessageID, arg.IsAllowed);
        }

        internal void StartAds(uint duration) => m_API!.StartCommercial(duration);

        private void PostMessage(string? message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                m_IRCAPI!.PostMessage(m_API!.GetSelfUserInfo(), message);
        }

        public CategoryInfo? SearchCategoryInfo(StreamGlass.Core.Controls.Window parent, CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info, m_API!);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        public void Test()
        {
            if (!m_IsConnected)
                return;
            TwitchUser m_StreamGlass = m_IRCAPI!.GetSelfUserInfo();
            TwitchUser m_Self = m_API!.GetSelfUserInfo();
            StreamGlassContext.LOGGER.Log("Testing Twitch");
            StreamGlassContext.LOGGER.Log("Testing follow");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(m_StreamGlass, new("J'aime le Pop-Corn"), 0, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 1");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(m_StreamGlass, new("J'aime le Pop-Corn"), 1, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 2");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(m_StreamGlass, new("J'aime le Pop-Corn"), 2, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing sub tier 3");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(m_StreamGlass, new("J'aime le Pop-Corn"), 3, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing prime sub");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(m_StreamGlass, new("J'aime le Pop-Corn"), 4, 69, 42));
            StreamGlassContext.LOGGER.Log("Testing gift sub tier 1");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.GIFT_FOLLOW, new GiftFollowEventArgs(m_Self, m_StreamGlass, new("Il aime le Pop-Corn"), 1, 69, 42, -1));
            StreamGlassContext.LOGGER.Log("Testing bits donation");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.DONATION, new DonationEventArgs(m_StreamGlass, 666, "bits", new("J'aime le Pop-Corn")));
            StreamGlassContext.LOGGER.Log("Testing incomming raid");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.RAID, new RaidEventArgs(m_StreamGlass, m_Self, 40, true));
            StreamGlassContext.LOGGER.Log("Testing reward");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.REWARD, new RewardEventArgs(m_StreamGlass, "Chante", "J'aime le Pop-Corn"));
            StreamGlassContext.LOGGER.Log("Testing shoutout");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.SHOUTOUT, new ShoutoutEventArgs(m_StreamGlass, m_Self));
            StreamGlassContext.LOGGER.Log("Testing incomming shoutout");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.BEING_SHOUTOUT, m_StreamGlass);
            StreamGlassContext.LOGGER.Log("Twitch tested");

            /*BytesWriter bytesWriter = m_EventSub.CreateBytesWriter();
            bytesWriter.Write(new Frame(true, 1, Encoding.UTF8.GetBytes("{\"metadata\":{\"message_id\":\"8tFzUmxgpeIhaX0wzBUKeA2KXpeWbc3gAQk7IUzaYBg=\",\"message_type\":\"notification\",\"message_timestamp\":\"2024-01-31T20:46:26.859382297Z\",\"subscription_type\":\"channel.chat.message\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"98860814-754a-4e0c-980b-510640ea008d\",\"status\":\"enabled\",\"type\":\"channel.chat.message\",\"version\":\"1\",\"condition\":{\"broadcaster_user_id\":\"52792239\",\"user_id\":\"52792239\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AgoQ0dkFoHiWS9mnjILZYMMazxIGY2VsbC1j\"},\"created_at\":\"2024-01-31T19:20:00.500247071Z\",\"cost\":0},\"event\":{\"broadcaster_user_id\":\"52792239\",\"broadcaster_user_login\":\"chaporon_\",\"broadcaster_user_name\":\"ChapORon_\",\"chatter_user_id\":\"737196630\",\"chatter_user_login\":\"dragshur\",\"chatter_user_name\":\"Dragshur\",\"message_id\":\"28f5dbe3-3543-4245-9497-e1e381f92238\",\"message\":{\"text\":\"Cheer10 GG\",\"fragments\":[{\"type\":\"cheermote\",\"text\":\"Cheer10\",\"cheermote\":{\"prefix\":\"cheer\",\"bits\":10,\"tier\":1},\"emote\":null,\"mention\":null},{\"type\":\"text\",\"text\":\" GG\",\"cheermote\":null,\"emote\":null,\"mention\":null}]},\"color\":\"#1E90FF\",\"badges\":[{\"set_id\":\"moderator\",\"id\":\"1\",\"info\":\"\"},{\"set_id\":\"founder\",\"id\":\"0\",\"info\":\"3\"},{\"set_id\":\"hype-train\",\"id\":\"1\",\"info\":\"\"}],\"message_type\":\"text\",\"cheer\":{\"bits\":10},\"reply\":null,\"channel_points_custom_reward_id\":null}}}")));
            bytesWriter.Write(new Frame(true, 1, Encoding.UTF8.GetBytes("{\"metadata\":{\"message_id\":\"ELY0WNH7y1zY9ZP3Apu3iqEowRboiV7BAtMJCyZLKvU=\",\"message_type\":\"notification\",\"message_timestamp\":\"2024-02-07T19:57:24.454Z\",\"subscription_type\":\"channel.subscribe\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"baa915fe-7c7d-4fd2-b8bb-ef0247d24eb6\",\"status\":\"enabled\",\"type\":\"channel.subscribe\",\"version\":\"1\",\"condition\":{\"broadcaster_user_id\":\"52792239\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AgoQpqLvYsrWRRaIr2aCLXyqwhIGY2VsbC1j\"},\"created_at\":\"2024-02-07T19:11:31.307943601Z\",\"cost\":0},\"event\":{\"user_id\":\"753317728\",\"user_login\":\"arecaceae_\",\"user_name\":\"arecaceae_\",\"broadcaster_user_id\":\"52792239\",\"broadcaster_user_login\":\"chaporon_\",\"broadcaster_user_name\":\"ChapORon_\",\"tier\":\"1000\",\"is_gift\":true}}}")));
            bytesWriter.Write(new Frame(true, 1, Encoding.UTF8.GetBytes("{\"metadata\":{\"message_id\":\"KyGdNhGCkwxCfojhRPEKsU1vLQKwJ31rDJiZevQBMms=\",\"message_type\":\"notification\",\"message_timestamp\":\"2024-02-07T19:57:24.818127579Z\",\"subscription_type\":\"channel.subscription.gift\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"a43950ab-9ff7-49d3-80f3-87c5bd91e382\",\"status\":\"enabled\",\"type\":\"channel.subscription.gift\",\"version\":\"1\",\"condition\":{\"broadcaster_user_id\":\"52792239\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AgoQpqLvYsrWRRaIr2aCLXyqwhIGY2VsbC1j\"},\"created_at\":\"2024-02-07T19:11:31.604837842Z\",\"cost\":0},\"event\":{\"user_id\":\"132973403\",\"user_login\":\"cloudwalker02\",\"user_name\":\"CloudWalker02\",\"broadcaster_user_id\":\"52792239\",\"broadcaster_user_login\":\"chaporon_\",\"broadcaster_user_name\":\"ChapORon_\",\"tier\":\"1000\",\"total\":1,\"cumulative_total\":5,\"is_anonymous\":false}}}")));
            bytesWriter.Write(new Frame(true, 1, Encoding.UTF8.GetBytes("{\"metadata\":{\"message_id\":\"7YjlQ1bd6q2zMaFtR8GP120XtM6pBn480uuP53ijlhM=\",\"message_type\":\"notification\",\"message_timestamp\":\"2024-02-07T19:57:25.019484848Z\",\"subscription_type\":\"channel.chat.notification\",\"subscription_version\":\"1\"},\"payload\":{\"subscription\":{\"id\":\"5acc2544-b6c5-4806-8417-53034715e329\",\"status\":\"enabled\",\"type\":\"channel.chat.notification\",\"version\":\"1\",\"condition\":{\"broadcaster_user_id\":\"52792239\",\"user_id\":\"52792239\"},\"transport\":{\"method\":\"websocket\",\"session_id\":\"AgoQpqLvYsrWRRaIr2aCLXyqwhIGY2VsbC1j\"},\"created_at\":\"2024-02-07T19:11:35.402900271Z\",\"cost\":0},\"event\":{\"broadcaster_user_id\":\"52792239\",\"broadcaster_user_login\":\"chaporon_\",\"broadcaster_user_name\":\"ChapORon_\",\"chatter_user_id\":\"132973403\",\"chatter_user_login\":\"cloudwalker02\",\"chatter_user_name\":\"CloudWalker02\",\"chatter_is_anonymous\":false,\"color\":\"#FF69B4\",\"badges\":[{\"set_id\":\"founder\",\"id\":\"0\",\"info\":\"17\"},{\"set_id\":\"bits-leader\",\"id\":\"1\",\"info\":\"\"}],\"system_message\":\"CloudWalker02 gifted a Tier 1 sub to arecaceae_! They have given 5 Gift Subs in the channel!\",\"message_id\":\"e63f0344-bfc0-436e-99ce-e66669985e68\",\"message\":{\"text\":\"\",\"fragments\":[]},\"notice_type\":\"sub_gift\",\"sub\":null,\"resub\":null,\"sub_gift\":{\"duration_months\":1,\"cumulative_total\":5,\"recipient_user_id\":\"753317728\",\"recipient_user_name\":\"arecaceae_\",\"recipient_user_login\":\"arecaceae_\",\"sub_tier\":\"1000\",\"community_gift_id\":null},\"community_sub_gift\":null,\"gift_paid_upgrade\":null,\"prime_paid_upgrade\":null,\"pay_it_forward\":null,\"raid\":null,\"unraid\":null,\"announcement\":null,\"bits_badge_tier\":null,\"charity_donation\":null}}}")));
            m_EventSub.TestRead(bytesWriter);*/
        }
    }
}
