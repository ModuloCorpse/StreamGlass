using CorpseLib;
using CorpseLib.Ini;
using CorpseLib.Web.OAuth;
using StreamGlass.Connections;
using StreamGlass.Controls;
using StreamGlass.Events;
using StreamGlass.Profile;
using StreamGlass.Settings;
using System.Collections.Generic;
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
            m_API.LoadGlobalChatBadges();
            TwitchUser? creator = m_API.GetUserInfoFromLogin("chaporon_");
            if (creator != null)
                m_API.LoadChannelEmoteSet(creator);

            TwitchUser? userInfoOfToken = m_API.GetUserInfoOfToken(m_IRCToken);
            if (userInfoOfToken != null)
                m_API.LoadEmoteSetFromFollowedChannel(userInfoOfToken);
            TwitchUser selfUserInfo = m_API.GetSelfUserInfo();
            m_API.LoadChannelChatBadges(selfUserInfo);
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
            TwitchUser m_StreamGlass = m_Chat.Self;
            TwitchUser m_Self = m_OriginalBroadcasterChannelInfo!.Broadcaster;
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 0, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 1, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 2, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 3, 69, 42));
            StreamGlassCanals.FOLLOW.Emit(new(m_StreamGlass, new("J'aime le Pop-Corn"), 4, 69, 42));
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(m_Self, m_StreamGlass, new("Il aime le Pop-Corn"), 1, 69, 42, -1));
            StreamGlassCanals.DONATION.Emit(new(m_StreamGlass, 666, "bits", new("J'aime le Pop-Corn")));
            StreamGlassCanals.RAID.Emit(new(m_StreamGlass, m_Self, 40, true));
            StreamGlassCanals.REWARD.Emit(new(m_StreamGlass, "Chante", "J'aime le Pop-Corn"));
            StreamGlassCanals.SHOUTOUT.Emit(new(m_StreamGlass, m_Self));

            List<string> tests = new()
            {
                "@badge-info=;badges=vip/1,artist-badge/1;color=#5F9EA0;display-name=arecaceae_;emotes=;first-msg=0;flags=;id=217a1866-58d0-4a95-ae76-471b8dddf9f4;mod=0;returning-chatter=0;room-id=52792239;subscriber=0;tmi-sent-ts=1700680360256;turbo=0;user-id=753317728;user-type=;vip=1 :arecaceae_!arecaceae_@arecaceae_.tmi.twitch.tv PRIVMSG #chaporon_ :Salut",
                "@badge-info=;badges=vip/1,artist-badge/1;color=#5F9EA0;display-name=arecaceae_;emotes=;first-msg=0;flags=;id=14616d77-0af1-4105-90bf-0690148078d0;mod=0;returning-chatter=0;room-id=52792239;subscriber=0;tmi-sent-ts=1700680374459;turbo=0;user-id=753317728;user-type=;vip=1 :arecaceae_!arecaceae_@arecaceae_.tmi.twitch.tv PRIVMSG #chaporon_ :Oui et toi ?",
                ":arecaceae_!arecaceae_@arecaceae_.tmi.twitch.tv JOIN #chaporon_",
                ":cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv JOIN #chaporon_",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=caf34c50-d6b3-4c34-931f-0f38caba2d0e;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700680726661;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :1) c’est quoi ce titre ? 2) Bonsoir",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=aba7bfb6-f31c-47ea-ae2c-dade5f4ec982;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700680779748;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Ca va un peu fatigué",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=3e8cea14-d863-4ead-ad77-600700df049c;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700680825347;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Encore une fois tu n’a pas posé la bonne question…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=6cc41570-4800-42a5-a519-b9a9363941c6;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700680851651;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :💩💩💩💩💩💩💩",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=555555589:64-65;first-msg=0;flags=;id=c1f3addb-7204-4faa-a71d-27aa6af772c8;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700680990740;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Je serais content quand tu auras fait une Game sur Dread Valley ;)",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=2c236beb-ec12-4ab0-b737-7fbd670a5bfc;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681011700;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Le mec qui en veut toujours plus…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=c7c72b3e-3a1b-4399-9385-f3cceb38d9d7;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681135588;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Si quelqu’un s’introduisait chez toi pour voler absolument TOUT ce qui est à l’intérieur je doute que tu arrives à rester calme…. Le Mejai c’est pareil",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=8030ba49-1c5c-4fb1-b39a-2aac95d39453;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681206291;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :En même temps quelle idée d’avoir un piaf de compagnie",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=f79a00fd-38a2-459a-a3ac-2e48b39c856e;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681301459;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Vivement le rework du bannissement et les nouveaux puzzles…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=ce590fe8-8db2-4e95-b973-d824800492f7;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681421428;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Ça m’étonnerait pas que ça sorte en janvier 2024 comme ils l’avaient fait avec le rework des Mejais cette année",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=5d25712b-a3b2-4395-bccc-7ed3decb0c41;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681469741;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :D’ailleurs Necreph n’a toujours pas sa nouvelle animation de mort…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=5005fa17-ccd8-4072-beef-fbebd5dd4290;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681492950;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :En moins de 9 minutes ça passe large",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=2fcfb05d-e974-4582-8893-a720fc464960;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681541505;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :…..",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=34-40:A.3,55-60:A.3;id=f5f1001f-ec72-4e18-aea5-fce843c04d38;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681577413;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Donnez argent pour soigner momies debiles (et un Chapo debile accessoirement)",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=c3c4c18a-f0c1-4f56-a220-7b09d1be1290;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681625496;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Et en plus tu prends un coup…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=92fea46b-266a-425d-b3d6-a29ee978cc3f;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681648896;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :C’est un de trop",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=ad4864c1-2134-4ac0-9542-16d0d9280050;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681667498;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Ça manquait de reverb",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=8294b586-6a21-4046-b145-40df8ca45185;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681688421;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :le premier hein pas le deuxième",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=60-65:P.6;id=ee0a491c-9049-4b8c-acdc-9b097cd202f5;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681753328;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :j’ai pas assez de points pour claquer un « Buvez de l’eau » PUTAIN",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=504a096b-1eb0-4188-8881-ffb066c3a687;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681819425;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Faut pas abuser non plus…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=b59dfccb-bac9-46ce-a2aa-b24422fb5162;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681839092;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Buvez pas de l’eau buvez Holy…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=e620a5ca-0d00-4721-8da5-4a0e393bb8bb;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681866603;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Ça m’arrangerait…",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=54d9080c-a742-480c-abcb-a412f93263e1;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681891044;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :ça me permettrait de te soutenir ET d’acheter mes boissons préférées moins cher",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=a70795f0-ca04-4256-83bb-2ecd083c10c7;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700681944953;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :En parlant d’escalade tu compte nous faire Jusant un de ces jours ?",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=59d0a5aa-d856-4abd-8e7f-36962a2b33a1;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682072737;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Capterge ?",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=dcd57968-d777-4217-87ca-3a6af0a2d267;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682151786;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Chapoforceur vibes intensifies",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=8b1cf876-cef5-467c-ad80-2faeccde0455;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682169852;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Je confirme",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=6ecb0582-1e93-40ea-a4d5-3d65bf3bb536;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682223043;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Cheminée de ses morts",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=c0d53187-f413-40a4-bcb0-cd9cdb64587d;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682259724;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Bonjour Ouphris",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=ac889c4f-eccf-4715-a2ee-d12d47c6d9e1;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682373445;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :C’était Matt dans le cercueil on est d’accord ?",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=cbeb5f46-c215-4b96-bad7-52b3105f3eda;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682448104;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Si Matt est dans le cercueil alors qui est dans les toilettes du lobby ? ….",
                "@badge-info=founder/1;badges=moderator/1,founder/0;client-nonce=80eab5f2cbf22159cc3907281946900a;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=b34cdbe0-2c3b-4b4b-a7a0-08d3455b5d25;mod=1;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682485697;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :Hello, ça va ?",
                "@badge-info=founder/1;badges=moderator/1,founder/0;client-nonce=4a35c9bdb4ce5583ce12b9859126ebe3;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=d5d4f90b-8959-407e-81a6-ee0c7c61bbfc;mod=1;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682491330;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :ça va",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=cd951cdd-e3a8-4c58-a599-a7c6ecc7fdea;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682493765;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :Salut Drag",
                "@badge-info=founder/1;badges=moderator/1,founder/0;client-nonce=152a2c6dda2ad953327f786793df7dd9;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=50da1ed7-69ca-49a7-b39a-302dde2698ea;mod=1;reply-parent-display-name=CloudWalker02;reply-parent-msg-body=Salut\\sDrag;reply-parent-msg-id=cd951cdd-e3a8-4c58-a599-a7c6ecc7fdea;reply-parent-user-id=132973403;reply-parent-user-login=cloudwalker02;reply-thread-parent-display-name=CloudWalker02;reply-thread-parent-msg-id=cd951cdd-e3a8-4c58-a599-a7c6ecc7fdea;reply-thread-parent-user-id=132973403;reply-thread-parent-user-login=cloudwalker02;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682502945;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :@CloudWalker02 ça va ?",
                "@badge-info=founder/15;badges=founder/0,premium/1;color=#FF69B4;display-name=CloudWalker02;emotes=;first-msg=0;flags=;id=343b9fc5-aad8-4f77-8620-cd83ff21522f;mod=0;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682521338;turbo=0;user-id=132973403;user-type= :cloudwalker02!cloudwalker02@cloudwalker02.tmi.twitch.tv PRIVMSG #chaporon_ :fatigué",
                "@badge-info=founder/1;badges=moderator/1,founder/0;client-nonce=f55adb4c84fd4b7b7cf4f616605ec5dc;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=fbd19bdf-6254-4bae-84f9-69bc534676fc;mod=1;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682528957;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :Ah",
                "@badge-info=founder/1;badges=moderator/1,founder/0;client-nonce=d0b9d81a1d587950df0847bdfa3f88b8;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=6e57a202-9da2-4f90-a7c3-6ff54a4c860e;mod=1;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682542725;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :Faut te reposer ^^",
                "@badge-info=founder/1;badges=moderator/1,founder/0;bits=1;color=#1E90FF;display-name=Dragshur;emotes=;first-msg=0;flags=;id=40a50bf4-3c8c-454f-9479-dd56136c4ded;mod=1;returning-chatter=0;room-id=52792239;subscriber=1;tmi-sent-ts=1700682556611;turbo=0;user-id=737196630;user-type=mod :dragshur!dragshur@dragshur.tmi.twitch.tv PRIVMSG #chaporon_ :Pour te consoler Cheer1"
            };
            //m_Chat.TestMessages(tests);
        }
    }
}
