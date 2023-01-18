using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.StreamChat;
using StreamFeedstock.Controls;
using ChatClient = StreamGlass.Twitch.IRC.ChatClient;
using StreamFeedstock;
using StreamGlass.StreamAlert;
using StreamGlass.Http;
using StreamGlass.Connections;
using StreamGlass.Settings;

namespace StreamGlass.Twitch
{
    public class Connection : IStreamConnection
    {
        private bool m_IsConnected = false;
        private string m_Channel = "";
        private readonly Settings.Data m_Settings;
        private readonly ChatClient m_Client;
        private ChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly Authenticator m_Authenticator;
        private readonly StreamGlassWindow m_Form;
        private readonly EventSub m_EventSub;

        public Connection(Server webServer, Settings.Data settings, StreamGlassWindow form)
        {
            m_Settings = settings;
            m_Form = form;
            m_Authenticator = new(webServer, settings);
            //This section will create the settings variables ONLY if they where not loaded
            m_Settings.Create("twitch", "auto_connect", "false");
            m_Settings.Create("twitch", "browser", "");
            m_Settings.Create("twitch", "public_key", "");
            m_Settings.Create("twitch", "secret_key", "");
            m_Settings.Create("twitch", "sub_mode", "all");

            m_Client = new(m_Settings);
            m_EventSub = new(m_Settings);

            if (m_Settings.Get("twitch", "auto_connect") == "true")
                Connect();
        }

        private void Register()
        {
            CanalManager.Register(StreamGlassCanals.CHAT_CONNECTED, OnConnected);
            CanalManager.Register<string>(StreamGlassCanals.CHAT_JOINED, OnJoinedChannel);
            CanalManager.Register<string>(StreamGlassCanals.USER_JOINED, OnUserJoinedChannel);
            CanalManager.Register<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
            CanalManager.Register<FollowEventArgs>(StreamGlassCanals.FOLLOW, OnNewFollow);
            CanalManager.Register<DonationEventArgs>(StreamGlassCanals.DONATION, OnDonation);
            CanalManager.Register<RaidEventArgs>(StreamGlassCanals.RAID, OnRaid);
            CanalManager.Register<RewardEventArgs>(StreamGlassCanals.REWARD, OnReward);

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
            CanalManager.Unregister<FollowEventArgs>(StreamGlassCanals.FOLLOW, OnNewFollow);
            CanalManager.Unregister<DonationEventArgs>(StreamGlassCanals.DONATION, OnDonation);
            CanalManager.Unregister<RaidEventArgs>(StreamGlassCanals.RAID, OnRaid);
            CanalManager.Unregister<RewardEventArgs>(StreamGlassCanals.REWARD, OnReward);
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
                    m_EventSub.SetToken(apiToken);
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
            else
            {
                m_Client.Reconnect();
            }
        }

        internal void ReconnectEventSub() => m_EventSub.Reconnect();

        public void Disconnect()
        {
            m_EventSub.Disconnect();
            m_Client.Disconnect();
            ResetStreamInfo();
            Unregister();
        }

        public TabItemContent[] GetSettings() => new TabItemContent[] { new TwitchSettingsItem(m_Settings, this) };

        private void OnConnected(int _)
        {
            UserInfo? selfUserInfo = API.GetSelfUserInfo();
            if (selfUserInfo != null)
                m_Client.Join(selfUserInfo.Login);
        }

        private void OnJoinedChannel(int _, object? arg)
        {
            if (arg == null)
                return;
            string channel = (string)arg!;
            if (m_Channel != channel)
            {
                m_Channel = channel;
                API.LoadChannelEmoteSetFromLogin(channel[1..]);
                ResetStreamInfo();
                m_OriginalBroadcasterChannelInfo = API.GetChannelInfoFromLogin(channel[1..]);
                m_Form.JoinChannel(channel);
                m_Client.SendMessage(channel, "Hello World! Je suis un bot connecté via StreamGlass!");
                if (m_OriginalBroadcasterChannelInfo != null)
                    m_EventSub.Connect(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
            }
        }

        public void Update(long _) {}

        private void OnUserJoinedChannel(int _, object? arg)
        {
            if (arg == null)
                return;
            string login = (string)arg!;
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

        private void SetStreamInfo(int _, object? arg)
        {
            if (arg == null)
                return;
            UpdateStreamInfoArgs args = (UpdateStreamInfoArgs)arg!;
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                if (string.IsNullOrWhiteSpace(args.Title) && string.IsNullOrWhiteSpace(args.Category.ID) && string.IsNullOrWhiteSpace(args.Language))
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
                else
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, args.Title, args.Category.ID, args.Language);
            }
        }

        private void OnNewFollow(int _, object? obj)
        {
            FollowEventArgs e = (FollowEventArgs)obj!;
            Alert? alert = e.Tier switch
            {
                0 => new("../Assets/hearts.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} is now following you: ", e.Name))),
                1 => new("../Assets/stars-stack-1.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} as subscribed to you with a tier 1: ", e.Name))),
                2 => new("../Assets/stars-stack-2.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} as subscribed to you with a tier 2: ", e.Name))),
                3 => new("../Assets/stars-stack-3.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} as subscribed to you with a tier 3: ", e.Name))),
                4 => new("../Assets/chess-queen.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} as subscribed to you with a prime: ", e.Name))),
                _ => null
            };
            if (alert != null)
                CanalManager.Emit(StreamGlassCanals.ALERT, alert);
        }

        private void OnDonation(int _, object? obj)
        {
            DonationEventArgs e = (DonationEventArgs)obj!;
            CanalManager.Emit(StreamGlassCanals.ALERT, new Alert("../Assets/take-my-money.png", DisplayableMessage.AppendPrefix(e.Message, string.Format("{0} as donated {1} {2}: ", e.Name, e.Amount, e.Currency))));
        }

        private void OnRaid(int _, object? obj)
        {
            RaidEventArgs e = (RaidEventArgs)obj!;
            if (e.ToID == m_OriginalBroadcasterChannelInfo!.Broadcaster.ID)
                CanalManager.Emit(StreamGlassCanals.ALERT, new Alert("../Assets/parachute.png", new(string.Format("{0} is raiding you with {1} viewers", e.From, e.NbViewers))));
            else
                CanalManager.Emit(StreamGlassCanals.ALERT, new Alert("../Assets/parachute.png", new(string.Format("You are raiding {0} with {1} viewers", e.To, e.NbViewers))));
        }

        private void OnReward(int _, object? obj)
        {
            RewardEventArgs e = (RewardEventArgs)obj!;
            CanalManager.Emit(StreamGlassCanals.ALERT, new Alert("../Assets/chest.png", new(string.Format("{0} retrieve {1}: {2}", e.From, e.Reward, e.Input))));
        }

        public Profile.CategoryInfo? SearchCategoryInfo(Window parent, Profile.CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        public void Test()
        {
        }
    }
}
