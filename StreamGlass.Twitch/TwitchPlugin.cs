using CorpseLib.Ini;
using CorpseLib.Json;
using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Audio;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Plugin;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Settings;
using StreamGlass.Twitch.Alert;
using StreamGlass.Twitch.API.Message;
using StreamGlass.Twitch.API.Timer;
using StreamGlass.Twitch.Commands;
using StreamGlass.Twitch.Events;
using StreamGlass.Twitch.Moderation;
using StreamGlass.Twitch.StreamChat;
using System.Globalization;
using System.IO;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class TwitchPlugin : APlugin
    {
        public static class Canals
        {
            public static readonly string CHAT_MESSAGE = "twitch_chat_message";
            public static readonly string CHAT_JOINED = "twitch_chat_joined";
            public static readonly string USER_JOINED = "twitch_user_joined";
            public static readonly string DONATION = "twitch_donation";
            public static readonly string FOLLOW = "twitch_follow";
            public static readonly string GIFT_FOLLOW = "twitch_gift_follow";
            public static readonly string RAID = "twitch_raid";
            public static readonly string REWARD = "twitch_reward";
            public static readonly string BAN = "twitch_ban";
            public static readonly string HELD_MESSAGE = "twitch_held_message";
            public static readonly string ALLOW_MESSAGE = "twitch_allow_message";
            public static readonly string HELD_MESSAGE_MODERATED = "twitch_held_message_moderated";
            public static readonly string CHAT_CLEAR_USER = "twitch_chat_clear_user";
            public static readonly string CHAT_CLEAR_MESSAGE = "twitch_chat_clear_message";
            public static readonly string SHOUTOUT = "twitch_shoutout";
            public static readonly string BEING_SHOUTOUT = "twitch_being_shoutout";
            public static readonly string CHAT_CLEAR = "twitch_chat_clear";
            public static readonly string STREAM_START = "twitch_stream_start";
            public static readonly string STREAM_STOP = "twitch_stream_stop";
        }

        //TODO Replace in all XAML use of TranslationKey attribute by the SetTranslationKey to keep track of used key
        public static class TranslationKeys
        {
            public static readonly TranslationKey SETTINGS_CHAT_MODE = new("settings_chat_mode");
            public static readonly TranslationKey SETTINGS_CHAT_FONT = new("settings_chat_font");
            public static readonly TranslationKey ALERT_EDITOR_ENABLE = new("alert_editor_enable");
            public static readonly TranslationKey ALERT_EDITOR_IMAGE = new("alert_editor_image");
            public static readonly TranslationKey ALERT_EDITOR_PREFIX = new("alert_editor_prefix");
            public static readonly TranslationKey SETTINGS_ALERT_ALERTS = new("settings_alert_alerts");
            public static readonly TranslationKey BAN_BUTTON = new("ban_button");
            public static readonly TranslationKey BAN_TIME = new("ban_time");
            public static readonly TranslationKey BAN_REASON = new("ban_reason");
            public static readonly TranslationKey SETTINGS_TWITCH_AUTOCONNECT = new("settings_twitch_autoconnect");
            public static readonly TranslationKey SETTINGS_TWITCH_CONNECT = new("settings_twitch_connect");
            public static readonly TranslationKey SETTINGS_TWITCH_BROWSER = new("settings_twitch_browser");
            public static readonly TranslationKey SETTINGS_TWITCH_BOT_PUBLIC = new("settings_twitch_bot_public");
            public static readonly TranslationKey SETTINGS_TWITCH_BOT_PRIVATE = new("settings_twitch_bot_private");
            public static readonly TranslationKey SETTINGS_TWITCH_SUB_MODE = new("settings_twitch_sub_mode");
            public static readonly TranslationKey SETTINGS_TWITCH_SEARCH = new("settings_twitch_search");
            public static readonly TranslationKey SETTINGS_TWITCH_DO_WELCOME = new("settings_twitch_do_welcome");
            public static readonly TranslationKey SETTINGS_TWITCH_WELCOME_MESSAGE = new("settings_twitch_welcome_message");
        }

        static TwitchPlugin()
        {
            JHelper.RegisterSerializer(new TwitchUser.JSerializer());
            JHelper.RegisterSerializer(new TwitchBadgeInfo.JSerializer());
            JHelper.RegisterSerializer(new BanEventArgs.JSerializer());
            JHelper.RegisterSerializer(new DonationEventArgs.JSerializer());
            JHelper.RegisterSerializer(new FollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new GiftFollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new MessageAllowedEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RaidEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RewardEventArgs.JSerializer());
            JHelper.RegisterSerializer(new ShoutoutEventArgs.JSerializer());
            JHelper.RegisterSerializer(new TwitchMessage.JSerializer());
        }

        private readonly TwitchCore m_Core = new();
        private readonly UserMessageScrollPanel m_StreamChatPanel;
        private readonly AlertScrollPanel m_StreamAlertPanel;
        private readonly HeldMessageScrollPanel m_HeldMessagePanel;
        private readonly IniFile m_Settings = new();

        public TwitchPlugin() : base("twitch")
        {
            m_StreamChatPanel = new();
            m_StreamAlertPanel = new();
            m_HeldMessagePanel = new();
            if (File.Exists("twitch_settings.ini"))
            {
                IniFile iniFile = IniFile.ParseFile("twitch_settings.ini");
                if (!iniFile.HaveEmptySection)
                    m_Settings.Merge(iniFile);
            }
        }

        protected override void InitTranslation()
        {
            Translation translation = new(new CultureInfo("en-US"), true)
            {
                { TranslationKeys.SETTINGS_CHAT_MODE, "Chat Mode:" },
                { TranslationKeys.SETTINGS_CHAT_FONT, "Chat Font Size:" },
                { TranslationKeys.ALERT_EDITOR_ENABLE, "Enable:" },
                { TranslationKeys.ALERT_EDITOR_IMAGE, "Image:" },
                { TranslationKeys.ALERT_EDITOR_PREFIX, "Prefix:" },
                { TranslationKeys.SETTINGS_ALERT_ALERTS, "Alerts" },
                { TranslationKeys.BAN_BUTTON, "Ban" },
                { TranslationKeys.BAN_TIME, "Time (s):" },
                { TranslationKeys.BAN_REASON, "Reason:" },
                { TranslationKeys.SETTINGS_TWITCH_AUTOCONNECT, "Auto-Connect:" },
                { TranslationKeys.SETTINGS_TWITCH_CONNECT, "Connect" },
                { TranslationKeys.SETTINGS_TWITCH_BROWSER, "Browser:" },
                { TranslationKeys.SETTINGS_TWITCH_BOT_PUBLIC, "Bot Public Key:" },
                { TranslationKeys.SETTINGS_TWITCH_BOT_PRIVATE, "Bot Secret Key:" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE, "Sub Mode:" },
                { TranslationKeys.SETTINGS_TWITCH_SEARCH, "Search:" },
                { TranslationKeys.SETTINGS_TWITCH_DO_WELCOME, "Do Welcome:" },
                { TranslationKeys.SETTINGS_TWITCH_WELCOME_MESSAGE, "Welcome Message:" }
            };
            Translator.AddTranslation(translation);
        }

        private void InitializeAlertSetting(IniSection alertSection, AlertScrollPanel.AlertType alertType, bool hasGift, Sound? audio, string imgPath, string prefix, string? chatMessage)
        {
            string audioFilePath = alertSection.GetOrAdd(string.Format("{0}_audio_file", alertType), audio?.File ?? string.Empty);
            string audioOutputPath = alertSection.GetOrAdd(string.Format("{0}_audio_output", alertType), audio?.Output ?? string.Empty);
            string loadedImgPath = alertSection.GetOrAdd(string.Format("{0}_path", alertType), imgPath);
            string loadedPrefix = alertSection.GetOrAdd(string.Format("{0}_prefix", alertType), prefix);
            string loadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_chat_message", alertType), chatMessage ?? string.Empty);
            bool loadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_enabled", alertType), "true") == "true";
            bool loadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_chat_message_enabled", alertType), (chatMessage != null) ? "true" : "false") == "true";
            m_StreamAlertPanel.SetAlertInfo(alertType, new(audioFilePath, audioOutputPath), loadedImgPath, loadedPrefix, loadedChatMessage, loadedIsEnabled, loadedChatMessageIsEnabled);

            if (hasGift)
            {
                string giftAudioFilePath = alertSection.GetOrAdd(string.Format("{0}_gift_audio_file", alertType), audio?.File ?? string.Empty);
                string giftAudioOutputPath = alertSection.GetOrAdd(string.Format("{0}_gift_audio_output", alertType), audio?.Output ?? string.Empty);
                string giftLoadedImgPath = alertSection.GetOrAdd(string.Format("{0}_gift_path", alertType), imgPath);
                string giftLoadedPrefix = alertSection.GetOrAdd(string.Format("{0}_gift_prefix", alertType), prefix);
                string giftLoadedChatMessage = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message", alertType), chatMessage ?? string.Empty);
                bool giftLoadedIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_enabled", alertType), "true") == "true";
                bool giftLoadedChatMessageIsEnabled = alertSection.GetOrAdd(string.Format("{0}_gift_chat_message_enabled", alertType), (chatMessage != null) ? "true" : "false") == "true";
                m_StreamAlertPanel.SetGiftAlertInfo(alertType, new(giftAudioFilePath, giftAudioOutputPath), giftLoadedImgPath, giftLoadedPrefix, giftLoadedChatMessage, giftLoadedIsEnabled, giftLoadedChatMessageIsEnabled);
            }
        }

        protected override void InitSettings()
        {
            IniSection settings = m_Settings.GetOrAdd("settings");
            settings.Add("auto_connect", "false");
            settings.Add("browser", string.Empty);
            settings.Add("public_key", string.Empty);
            settings.Add("secret_key", string.Empty);
            settings.Add("sub_mode", "all");
            settings.Add("do_welcome", "true");
            settings.Add("welcome_message", "Hello World! I'm a bot connected with StreamGlass!");
            m_Core.SetSettings(settings);

            //Chat
            IniSection chatSection = m_Settings.GetOrAdd("chat");
            m_StreamChatPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(chatSection.GetOrAdd("display_type", "0")));
            m_StreamChatPanel.SetContentFontSize(double.Parse(chatSection.GetOrAdd("message_font_size", "14")));
            chatSection.Add("do_welcome", "true");
            chatSection.Add("welcome_message", "Hello World! Je suis un bot connecté via StreamGlass!");

            //Alert
            IniSection alertSection = m_Settings.GetOrAdd("alert");
            m_StreamAlertPanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(alertSection.GetOrAdd("display_type", "0")));
            m_StreamAlertPanel.SetContentFontSize(double.Parse(alertSection.GetOrAdd("message_font_size", "20")));
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.INCOMMING_RAID, false, null, "../Assets/parachute.png", "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.OUTGOING_RAID, false, null, "../Assets/parachute.png", "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.DONATION, false, null, "../Assets/take-my-money.png", "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.REWARD, false, new("Assets/alert-sound.wav", string.Empty), "../Assets/chest.png", "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.FOLLOW, false, null, "../Assets/hearts.png", "${e.User.DisplayName} is now following you: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.TIER1, true, null, "../Assets/stars-stack-1.png", "${e.User.DisplayName} as subscribed to you with a tier 1: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.TIER2, true, null, "../Assets/stars-stack-2.png", "${e.User.DisplayName} as subscribed to you with a tier 2: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.TIER3, true, null, "../Assets/stars-stack-3.png", "${e.User.DisplayName} as subscribed to you with a tier 3: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.TIER4, true, null, "../Assets/chess-queen.png", "${e.User.DisplayName} as subscribed to you with a prime: ", null);
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", "Go check ${DisplayName(Lower(e.User.DisplayName))}, who's playing ${Game(e.User.DisplayName)} on https://twitch.tv/${e.User.Name}");
            InitializeAlertSetting(alertSection, AlertScrollPanel.AlertType.BEING_SHOUTOUT, false, null, "../Assets/megaphone.png", "${e.DisplayName} gave you a shoutout", null);

            //Held message
            IniSection moderationSection = m_Settings.GetOrAdd("moderation");
            m_HeldMessagePanel.SetDisplayType((ScrollPanelDisplayType)int.Parse(moderationSection.GetOrAdd("display_type", "0")));
            m_HeldMessagePanel.SetSenderFontSize(double.Parse(moderationSection.GetOrAdd("sender_font_size", "14")));
            m_HeldMessagePanel.SetSenderWidth(double.Parse(moderationSection.GetOrAdd("sender_size", "200")));
            m_HeldMessagePanel.SetContentFontSize(double.Parse(moderationSection.GetOrAdd("message_font_size", "14")));
        }

        protected override void InitPlugin()
        {
            StreamGlassContext.CreateStatistic("viewer_count");
            StreamGlassContext.CreateStatistic("last_bits_donor");
            StreamGlassContext.CreateStatistic("last_bits_donation");
            StreamGlassContext.CreateStatistic("top_bits_donor");
            StreamGlassContext.CreateStatistic("top_bits_donation");
            StreamGlassContext.CreateStatistic("last_follow");
            StreamGlassContext.CreateStatistic("last_raider");
            StreamGlassContext.CreateStatistic("last_gifter");
            StreamGlassContext.CreateStatistic("last_nb_gift");
            StreamGlassContext.CreateStatistic("top_gifter");
            StreamGlassContext.CreateStatistic("top_nb_gift");
            StreamGlassContext.CreateStatistic("last_sub");

            m_Core.OnPluginInit();
            if (m_Settings.Get("settings")!.Get("auto_connect") == "true")
                m_Core.Connect();
            m_HeldMessagePanel.Init();
            m_StreamChatPanel.Init();
            m_StreamAlertPanel.Init();
            ProfileEditor.SetSearchCategoryDelegate(m_Core.SearchCategoryInfo);
        }

        protected override void InitCommands()
        {
            //TODO Add API command
            StreamGlassCLI.AddCommand(new TwitchAd(m_Core));
        }

        protected override void InitCanals()
        {
            StreamGlassCanals.NewCanal<TwitchMessage>(Canals.CHAT_MESSAGE);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_JOINED);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.USER_JOINED);
            StreamGlassCanals.NewCanal<DonationEventArgs>(Canals.DONATION);
            StreamGlassCanals.NewCanal<FollowEventArgs>(Canals.FOLLOW);
            StreamGlassCanals.NewCanal<GiftFollowEventArgs>(Canals.GIFT_FOLLOW);
            StreamGlassCanals.NewCanal<RaidEventArgs>(Canals.RAID);
            StreamGlassCanals.NewCanal<RewardEventArgs>(Canals.REWARD);
            StreamGlassCanals.NewCanal<BanEventArgs>(Canals.BAN);
            StreamGlassCanals.NewCanal<TwitchMessage>(Canals.HELD_MESSAGE);
            StreamGlassCanals.NewCanal<MessageAllowedEventArgs>(Canals.ALLOW_MESSAGE);
            StreamGlassCanals.NewCanal<string>(Canals.HELD_MESSAGE_MODERATED);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_CLEAR_USER);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_CLEAR_MESSAGE);
            StreamGlassCanals.NewCanal<ShoutoutEventArgs>(Canals.SHOUTOUT);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.BEING_SHOUTOUT);
            StreamGlassCanals.NewCanal(Canals.CHAT_CLEAR);
            StreamGlassCanals.NewCanal(Canals.STREAM_START);
            StreamGlassCanals.NewCanal(Canals.STREAM_STOP);
        }

        protected override void RegisterAPI(CorpseLib.Web.API.API api)
        {
            api.AddEndpoint(new AllMessageEndpoint());
            api.AddEndpoint(new ClearMessageEndpoint());

            //TODO Move in a dedicated plugin
            api.AddEndpoint(new TimerEndpoint());
        }

        protected override void Unregister()
        {
            m_Core.Disconnect();
            m_Settings.WriteToFile("twitch_settings.ini");
        }

        protected override void Update(long deltaTime) { }

        protected override TabItemContent[] GetSettings() => [
            new StreamChatSettingsItem(m_Settings.GetOrAdd("chat"), m_StreamChatPanel),
            new StreamAlertSettingsItem(m_Settings.GetOrAdd("alert"), m_StreamAlertPanel),
            new TwitchSettingsItem(m_Settings.GetOrAdd("settings"), m_Core)
        ];

        protected override void TestPlugin() => m_Core.Test();

        public AlertScrollPanel CreateAlertScrollPanel() => m_StreamAlertPanel;
        public HeldMessageScrollPanel CreateHeldMessageScrollPanel() => m_HeldMessagePanel;
        public UserMessageScrollPanel CreateUserMessageScrollPanel() => m_StreamChatPanel;
    }
}
