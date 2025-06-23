using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.Network;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;
using StreamGlass.Core.StreamChat;
using StreamGlass.Twitch.Events;
using System.IO;
using TwitchCorpse;
using TwitchCorpse.API;
using static TwitchCorpse.TwitchEventSub;

namespace StreamGlass.Twitch
{
    public class Core : IMessageSourceHandler
    {
        private static readonly string AUTHENTICATOR_PAGE_CONTENT = "<!DOCTYPE html><html><head><title>StreamGlass Twitch Auth</title></head><body><p>You can close this page</p></body></html>";

        private readonly ChannelRewardManager m_ChannelRewardManager = new();
        private readonly RecurringAction m_GetViewerCount = new(500);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private Settings m_Settings = null;
        private Handler m_TwitchHandler = null;
        private TwitchEventSub m_EventSub = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private TwitchAPI? m_BroadcasterAPI;
        private TwitchAPI? m_ChatterAPI;
        private bool m_IsConnected = false;
        private bool m_IsSameAPI = true;
        private readonly SubscriptionType[] m_SubscriptionTypes = [
            SubscriptionType.ChannelFollow,
            SubscriptionType.ChannelSubscribe,
            SubscriptionType.ChannelSubscriptionGift,
            SubscriptionType.ChannelRaid,
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
            SubscriptionType.AutomodMessageUpdate,
            SubscriptionType.SharedChatBegin,
            SubscriptionType.SharedChatEnd,
            SubscriptionType.ChannelPointsAutomaticRewardRedemptionAdd,
            SubscriptionType.ChannelPointsCustomRewardRedemptionAdd,
            SubscriptionType.ChannelPointsCustomRewardAdd,
            SubscriptionType.ChannelPointsCustomRewardRemove,
            SubscriptionType.ChannelPointsCustomRewardUpdate
        ];

        public Core()
        {
            TwitchAPI.StartLogging();
            TwitchEventSub.StartLogging();
            m_GetViewerCount.OnUpdate += UpdateViewerCount;
        }

        public bool IsSelf(TwitchUser user) => user.ID == m_BroadcasterAPI!.GetSelfUserInfo().ID;
        public bool IsIRCSelf(TwitchUser user) => user.ID == m_ChatterAPI!.GetSelfUserInfo().ID;

        internal void SetSettings(Settings settings) => m_Settings = settings;

        private bool Authenticate(string browser)
        {
            m_BroadcasterAPI!.AuthenticateFromVault(StreamGlassVault.Vault, TwitchPlugin.VaultKeys.API_TOKEN);
            if (!m_BroadcasterAPI.IsAuthenticated)
                m_BroadcasterAPI.AuthenticateWithBrowser();
            if (m_BroadcasterAPI.IsAuthenticated)
                m_BroadcasterAPI.AppAuthenticate();
            if (string.IsNullOrWhiteSpace(browser))
            {
                m_ChatterAPI = m_BroadcasterAPI;
                m_IsSameAPI = true;
            }
            else
            {
                m_ChatterAPI!.AuthenticateFromVault(StreamGlassVault.Vault, TwitchPlugin.VaultKeys.IRC_TOKEN);
                if (!m_ChatterAPI.IsAuthenticated)
                    m_ChatterAPI.AuthenticateWithBrowser(browser);
                m_IsSameAPI = false;
            }
            m_ChatterAPI.AppAuthenticate(m_BroadcasterAPI);
            if (File.Exists("twitch_api_token"))
                File.Delete("twitch_api_token");
            if (File.Exists("twitch_irc_api_token"))
                File.Delete("twitch_irc_api_token");
            return m_BroadcasterAPI.IsAuthenticated && m_ChatterAPI.IsAuthenticated;
        }

        private bool Init()
        {
            if (m_BroadcasterAPI == null || !m_BroadcasterAPI.IsAuthenticated || m_ChatterAPI == null || !m_ChatterAPI.IsAuthenticated)
                return false;
            //TODO generate path to twitch logo
            string twitchLogoPath = string.Empty;
            MessageSource twitchMessageSource = StreamGlassChat.GetOrCreateMessageSource("twitch", "Twitch chat", twitchLogoPath, this);
            m_TwitchHandler = new Handler(this, m_Settings, m_BroadcasterAPI, twitchMessageSource);
            twitchMessageSource.RegisterChatContextMenu(TwitchPlugin.TranslationKeys.MENU_BAN, BanUserFromMessage);
            twitchMessageSource.RegisterChatContextMenu(TwitchPlugin.TranslationKeys.MENU_SHOUTOUT, ShoutoutUserFromMessage);

            m_BroadcasterAPI.SetHandler(m_TwitchHandler);
            m_BroadcasterAPI.ResetCache();

            TwitchUser broadcasterInfo = m_BroadcasterAPI.GetSelfUserInfo();
            string broadcasterName = broadcasterInfo.Name;
            if (!string.IsNullOrEmpty(broadcasterName))
            {
                m_TwitchHandler.SetIRCChannel(broadcasterName);
                StreamGlassContext.RegisterVariable("Self", broadcasterName);
            }

            m_ChannelRewardManager.AddChannelRewards(m_BroadcasterAPI!.GetChannelRewards());

            TwitchEventSub? eventSub = m_BroadcasterAPI.EventSubConnection(broadcasterInfo.ID, m_SubscriptionTypes);
            if (eventSub != null)
            {
                m_EventSub = eventSub;
                m_EventSub.SetMonitor(new DebugLogMonitor(TwitchEventSub.LOGGER));
                m_EventSub.OnWelcome += (object? sender, EventArgs e) =>
                {
                    if (m_Settings.DoWelcome)
                        SendMessage(m_Settings.WelcomeMessage, true);
                };
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
            StreamGlassCanals.Register<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
            StreamGlassCanals.Register<MessageAllowedEventArgs>(TwitchPlugin.Canals.ALLOW_MESSAGE, AllowMessage);
        }

        private TwitchUser? GetUserInfo(string login, Cache cache)
        {
            string cacheValueName = string.Format("{0}_twitch_user", login);
            TwitchUser? user = cache.GetCachedValue<TwitchUser>(cacheValueName);
            if (user == null)
            {
                user = m_BroadcasterAPI!.GetUserInfoFromLogin(login);
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
                channelInfo = m_BroadcasterAPI!.GetChannelInfo(user);
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
                categoryInfo = m_BroadcasterAPI!.GetCategoryInfo(channelInfo.GameID, channelInfo.GameName);
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
                string secretKey = StreamGlassVault.Load(TwitchPlugin.VaultKeys.SECRET);
                m_BroadcasterAPI = new(m_Settings.PublicKey, secretKey, 3000, AUTHENTICATOR_PAGE_CONTENT);
                m_ChatterAPI = new(m_Settings.PublicKey, secretKey, 3000, AUTHENTICATOR_PAGE_CONTENT);
                if (!Authenticate(m_Settings.Browser))
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
                m_TwitchHandler.UnregisterChatContextMenu(TwitchPlugin.TranslationKeys.MENU_BAN);
                m_TwitchHandler.UnregisterChatContextMenu(TwitchPlugin.TranslationKeys.MENU_SHOUTOUT);
                m_BroadcasterAPI!.SaveAPIToken(StreamGlassVault.Vault, TwitchPlugin.VaultKeys.API_TOKEN);
                if (!m_IsSameAPI)
                    m_ChatterAPI!.SaveAPIToken(StreamGlassVault.Vault, TwitchPlugin.VaultKeys.IRC_TOKEN);
                m_GetViewerCount.Stop();
                m_EventSub?.Disconnect();
                m_IsConnected = false;
                StreamGlassCanals.Unregister<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
                StreamGlassCanals.Unregister<MessageAllowedEventArgs>(TwitchPlugin.Canals.ALLOW_MESSAGE, AllowMessage);
                StreamGlassContext.UnregisterFunction("Game");
                StreamGlassContext.UnregisterFunction("DisplayName");
                StreamGlassContext.UnregisterFunction("Channel");
                StreamGlassContext.UnregisterFunction("Avatar");
            }
        }

        private void UpdateViewerCount(object? sender, EventArgs e)
        {
            TwitchChannelInfo? currentBroadcasterChannelInfo = m_BroadcasterAPI!.GetChannelInfo();
            if (currentBroadcasterChannelInfo != null)
            {
                if (m_TwitchHandler.UpdateViewerCountOf(currentBroadcasterChannelInfo.Broadcaster))
                    return;
            }
            //Stopping viewer count action as either information are missing or stream isn't started yet
            m_GetViewerCount.Stop();
        }

        internal void AddChannelReward(TwitchRewardInfo info) => m_ChannelRewardManager.AddChannelReward(info);
        internal void RemoveChannelReward(string rewardID) => m_ChannelRewardManager.DeleteChannelReward(rewardID);

        internal void ClaimChannelReward(TwitchUser user, TwitchRewardRedemptionInfo redemption, Text input) => m_ChannelRewardManager.ClaimReward(user, redemption, input);

        internal void OnStreamStart() => m_GetViewerCount.Start();

        private void SetStreamInfo(UpdateStreamInfoArgs? arg)
        {
            if (arg == null)
                return;
            TwitchChannelInfo? currentBroadcasterChannelInfo = m_BroadcasterAPI!.GetChannelInfo();
            if (currentBroadcasterChannelInfo != null)
            {
                string title = (string.IsNullOrWhiteSpace(arg.Title)) ? currentBroadcasterChannelInfo.Title : arg.Title;
                string category = (string.IsNullOrWhiteSpace(arg.Category.ID)) ? currentBroadcasterChannelInfo.GameID : arg.Category.ID;
                string language = (string.IsNullOrWhiteSpace(arg.Language)) ? currentBroadcasterChannelInfo.BroadcasterLanguage : arg.Language;
                m_BroadcasterAPI!.SetChannelInfo(currentBroadcasterChannelInfo.Broadcaster, title, category, language);
            }
        }

        private void AllowMessage(MessageAllowedEventArgs? arg)
        {
            if (arg == null)
                return;
            m_BroadcasterAPI!.ManageHeldMessage(arg.MessageID, arg.IsAllowed);
        }

        internal void StartAds(uint duration) => m_BroadcasterAPI!.StartCommercial(duration);

        private void SendMessage(string message, bool forSourceOnly) => m_ChatterAPI!.PostMessage(m_BroadcasterAPI!.GetSelfUserInfo(), message, forSourceOnly);

        public void SendMessage(DataObject messageData)
        {
            if (m_ChatterAPI == null || !m_ChatterAPI.IsAuthenticated)
                return;
            if (messageData.TryGet("message", out string? message) && !string.IsNullOrWhiteSpace(message))
                SendMessage(message, messageData.GetOrDefault("for_source_only", false));
        }

        public void DeleteMessage(string id)
        {
            string twitchID = m_TwitchHandler.GetMessageID(id);
            if (!string.IsNullOrEmpty(twitchID))
            {
                m_BroadcasterAPI!.DeleteMessage(twitchID);
                m_TwitchHandler.RemoveMessage(twitchID);
            }
        }

        public CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info, m_BroadcasterAPI!);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        public void BanUserFromMessage(Window window, StreamGlass.Core.StreamChat.Message message)
        {
            TwitchUser? user = m_TwitchHandler.GetUserFromMessage(message);
            if (user != null)
            {
                if (user.UserType == TwitchUser.Type.SELF)
                    return;
                BanDialog dialog = new(window, user);
                dialog.ShowDialog();
                BanEventArgs? arg = dialog.Event;
                if (arg != null)
                    m_BroadcasterAPI!.BanUser(arg.User, arg.Reason, arg.Delay);
            }
        }

        public void ShoutoutUserFromMessage(Window window, StreamGlass.Core.StreamChat.Message message)
        {
            TwitchUser? user = m_TwitchHandler.GetUserFromMessage(message);
            if (user != null)
                m_BroadcasterAPI!.ShoutoutUser(user);
        }

        public void Test()
        {
            if (!m_IsConnected)
                return;
            TwitchUser m_StreamGlass = m_ChatterAPI!.GetSelfUserInfo();
            TwitchUser m_Self = m_BroadcasterAPI!.GetSelfUserInfo();
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
            StreamGlassCanals.Emit(TwitchPlugin.Canals.REWARD, new RewardEventArgs(m_StreamGlass, "Chante", [new TextSection("J'aime le Pop-Corn")]));
            StreamGlassContext.LOGGER.Log("Testing shoutout");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.SHOUTOUT, new ShoutoutEventArgs(m_StreamGlass, m_Self));
            StreamGlassContext.LOGGER.Log("Testing incomming shoutout");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.BEING_SHOUTOUT, m_StreamGlass);
            StreamGlassContext.LOGGER.Log("Twitch tested");
        }
    }
}
