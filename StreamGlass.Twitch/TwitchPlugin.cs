using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API;
using StreamGlass.Core;
using StreamGlass.Core.Plugin;
using StreamGlass.Core.Profile;
using StreamGlass.Core.Settings;
using StreamGlass.Twitch.Actions;
using StreamGlass.Twitch.Alerts;
using StreamGlass.Twitch.API.Message;
using StreamGlass.Twitch.Events;
using StreamGlass.Twitch.Moderation;
using StreamGlass.Twitch.StreamChat;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class TwitchPlugin() : APlugin("Twitch"), IAPIPlugin, ITestablePlugin, ISettingsPlugin, IPanelPlugin
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
            public static readonly string ALERT = "twitch_stream_alert";
            public static readonly string ALLOW_AUTOMOD = "twitch_allow_automod";
        }

        public static class TranslationKeys
        {
            public static readonly TranslationKey SETTINGS_CHAT_MODE = new("twitch_settings_chat_mode");
            public static readonly TranslationKey SETTINGS_CHAT_FONT = new("twitch_settings_chat_font");
            public static readonly TranslationKey ALERT_EDITOR_ENABLE = new("twitch_alert_editor_enable");
            public static readonly TranslationKey ALERT_EDITOR_IMAGE = new("twitch_alert_editor_image");
            public static readonly TranslationKey ALERT_EDITOR_PREFIX = new("twitch_alert_editor_prefix");
            public static readonly TranslationKey SETTINGS_ALERT_ALERTS = new("twitch_settings_alert_alerts");
            public static readonly TranslationKey MENU_BAN = new("twitch_menu_ban");
            public static readonly TranslationKey BAN_BUTTON = new("twitch_ban_button");
            public static readonly TranslationKey BAN_TIME = new("twitch_ban_time");
            public static readonly TranslationKey BAN_REASON = new("twitch_ban_reason");
            public static readonly TranslationKey SETTINGS_TWITCH_AUTOCONNECT = new("twitch_settings_twitch_autoconnect");
            public static readonly TranslationKey SETTINGS_TWITCH_CONNECT = new("twitch_settings_twitch_connect");
            public static readonly TranslationKey SETTINGS_TWITCH_BROWSER = new("twitch_settings_twitch_browser");
            public static readonly TranslationKey SETTINGS_TWITCH_BOT_PUBLIC = new("twitch_settings_twitch_bot_public");
            public static readonly TranslationKey SETTINGS_TWITCH_BOT_PRIVATE = new("twitch_settings_twitch_bot_private");
            public static readonly TranslationKey SETTINGS_TWITCH_SUB_MODE = new("twitch_settings_twitch_sub_mode");
            public static readonly TranslationKey SETTINGS_TWITCH_SEARCH = new("twitch_settings_twitch_search");
            public static readonly TranslationKey SETTINGS_TWITCH_DO_WELCOME = new("twitch_settings_twitch_do_welcome");
            public static readonly TranslationKey SETTINGS_TWITCH_WELCOME_MESSAGE = new("twitch_settings_twitch_welcome_message");
            public static readonly TranslationKey SETTINGS_TWITCH_SUB_MODE_CLAIMED = new("twitch_settings_twitch_sub_mode");
            public static readonly TranslationKey SETTINGS_TWITCH_SUB_MODE_ALL = new("twitch_settings_twitch_sub_mode");
            public static readonly TranslationKey DISPLAY_TYPE_TTB = new("twitch_display_type_ttb");
            public static readonly TranslationKey DISPLAY_TYPE_RTTB = new("twitch_display_type_rttb");
            public static readonly TranslationKey DISPLAY_TYPE_BTT = new("twitch_display_type_btt");
            public static readonly TranslationKey DISPLAY_TYPE_RBTT = new("twitch_display_type_rbtt");
            public static readonly TranslationKey ALERT_NAME_INC_RAID = new("twitch_alert_name_inc_raid");
            public static readonly TranslationKey ALERT_NAME_OUT_RAID = new("twitch_alert_name_out_raid");
            public static readonly TranslationKey ALERT_NAME_DONATION = new("twitch_alert_name_donation");
            public static readonly TranslationKey ALERT_NAME_REWARD = new("twitch_alert_name_reward");
            public static readonly TranslationKey ALERT_NAME_FOLLOW = new("twitch_alert_name_follow");
            public static readonly TranslationKey ALERT_NAME_SUB_TIER1 = new("twitch_alert_name_sub_tier1");
            public static readonly TranslationKey ALERT_NAME_SUB_TIER2 = new("twitch_alert_name_sub_tier2");
            public static readonly TranslationKey ALERT_NAME_SUB_TIER3 = new("twitch_alert_name_sub_tier3");
            public static readonly TranslationKey ALERT_NAME_GIFT_SUB_TIER1 = new("twitch_alert_name_gift_sub_tier1");
            public static readonly TranslationKey ALERT_NAME_GIFT_SUB_TIER2 = new("twitch_alert_name_gift_sub_tier2");
            public static readonly TranslationKey ALERT_NAME_GIFT_SUB_TIER3 = new("twitch_alert_name_gift_sub_tier3");
            public static readonly TranslationKey ALERT_NAME_PRIME = new("twitch_alert_name_prime");
            public static readonly TranslationKey ALERT_NAME_SHOUTOUT = new("twitch_alert_name_shoutout");
            public static readonly TranslationKey ALERT_NAME_BEING_SHOUTOUT = new("twitch_alert_name_being_shoutout");
            public static readonly TranslationKey ALERT_EDITOR_AUDIO_FILE = new("twitch_alert_editor_audio_file");
            public static readonly TranslationKey ALERT_CHAT_MESSAGE = new("twitch_alert_chat_message");
        }

        private static readonly Metadata ms_PluginMetadata;

        public static Metadata PluginMetadata => ms_PluginMetadata;

        static TwitchPlugin()
        {
            ms_PluginMetadata = Metadata.CreateNativeMetadata<TwitchPlugin>("StreamGlass.Twitch.dll");

            JsonHelper.RegisterSerializer(new TwitchUser.JSerializer());
            JsonHelper.RegisterSerializer(new TwitchBadgeInfo.JSerializer());
            JsonHelper.RegisterSerializer(new BanEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new DonationEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new FollowEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new GiftFollowEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new MessageAllowedEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new RaidEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new RewardEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new ShoutoutEventArgs.JSerializer());
            JsonHelper.RegisterSerializer(new Message.JSerializer());
            JsonHelper.RegisterSerializer(new VisualAlert.JSerializer());

            JsonHelper.RegisterSerializer(new AlertSettings.JsonSerializer());
            JsonHelper.RegisterSerializer(new Settings.AlertsSettings.JsonSerializer());
            JsonHelper.RegisterSerializer(new Settings.ChatSettings.JsonSerializer());
            JsonHelper.RegisterSerializer(new Settings.ModerationSettings.JsonSerializer());
            JsonHelper.RegisterSerializer(new Settings.JsonSerializer());
        }

        private readonly Core m_Core = new();
        private readonly AlertManager m_AlertManager = new();
        private readonly UserMessageScrollPanel m_StreamChatPanel = new();
        private readonly AlertScrollPanel m_StreamAlertPanel = new();
        private readonly HeldMessageScrollPanel m_HeldMessagePanel = new();
        private Settings m_Settings = new();

        protected override PluginInfo GeneratePluginInfo() => new("1.0.0-beta", "ModuloCorpse<https://www.twitch.tv/chaporon_>");

        private static void InitTranslation()
        {
            Translation translation = new(new CultureInfo("en-US"), true)
            {
                { TranslationKeys.SETTINGS_CHAT_MODE, "Chat Mode:" },
                { TranslationKeys.SETTINGS_CHAT_FONT, "Chat Font Size:" },
                { TranslationKeys.ALERT_EDITOR_ENABLE, "Enable:" },
                { TranslationKeys.ALERT_EDITOR_IMAGE, "Image:" },
                { TranslationKeys.ALERT_EDITOR_PREFIX, "Prefix:" },
                { TranslationKeys.SETTINGS_ALERT_ALERTS, "Alerts" },
                { TranslationKeys.MENU_BAN, "Ban User" },
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
                { TranslationKeys.SETTINGS_TWITCH_WELCOME_MESSAGE, "Welcome Message:" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE_CLAIMED, "Claimed" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE_ALL, "All" },
                { TranslationKeys.DISPLAY_TYPE_TTB, "Top to bottom" },
                { TranslationKeys.DISPLAY_TYPE_RTTB, "Top to bottom reversed" },
                { TranslationKeys.DISPLAY_TYPE_BTT, "Bottom to top" },
                { TranslationKeys.DISPLAY_TYPE_RBTT, "Bottom to top reversed" },
                { TranslationKeys.ALERT_NAME_INC_RAID, "Incomming raid" },
                { TranslationKeys.ALERT_NAME_OUT_RAID, "Outgoing raid" },
                { TranslationKeys.ALERT_NAME_DONATION, "Donation" },
                { TranslationKeys.ALERT_NAME_REWARD, "Reward" },
                { TranslationKeys.ALERT_NAME_FOLLOW, "Follow" },
                { TranslationKeys.ALERT_NAME_SUB_TIER1, "Tier 1" },
                { TranslationKeys.ALERT_NAME_SUB_TIER2, "Tier 2" },
                { TranslationKeys.ALERT_NAME_SUB_TIER3, "Tier 3" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER1, "Gift tier 1" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER2, "Gift tier 2" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER3, "Gift tier 3" },
                { TranslationKeys.ALERT_NAME_PRIME, "Prime" },
                { TranslationKeys.ALERT_NAME_SHOUTOUT, "Shoutout" },
                { TranslationKeys.ALERT_NAME_BEING_SHOUTOUT, "Being shoutout" },
                { TranslationKeys.ALERT_EDITOR_AUDIO_FILE, "Audio file:" },
                { TranslationKeys.ALERT_CHAT_MESSAGE, "Chat message" }
            };
            Translator.AddTranslation(translation);
            Translation translationFR = new(new CultureInfo("fr-FR"), true)
            {
                { TranslationKeys.SETTINGS_CHAT_MODE, "Mode du Chat :" },
                { TranslationKeys.SETTINGS_CHAT_FONT, "Taille du Chat :" },
                { TranslationKeys.ALERT_EDITOR_ENABLE, "Activer :" },
                { TranslationKeys.ALERT_EDITOR_IMAGE, "Image :" },
                { TranslationKeys.ALERT_EDITOR_PREFIX, "Alerte :" },
                { TranslationKeys.SETTINGS_ALERT_ALERTS, "Alertes" },
                { TranslationKeys.MENU_BAN, "Bannir l'utilisateur" },
                { TranslationKeys.BAN_BUTTON, "Bannir" },
                { TranslationKeys.BAN_TIME, "Durée (s) :" },
                { TranslationKeys.BAN_REASON, "Raison :" },
                { TranslationKeys.SETTINGS_TWITCH_AUTOCONNECT, "Connection Automatique :" },
                { TranslationKeys.SETTINGS_TWITCH_CONNECT, "Connection" },
                { TranslationKeys.SETTINGS_TWITCH_BROWSER, "Navigateur :" },
                { TranslationKeys.SETTINGS_TWITCH_BOT_PUBLIC, "Clé Publique :" },
                { TranslationKeys.SETTINGS_TWITCH_BOT_PRIVATE, "Clé Privée :" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE, "Mode de sub :" },
                { TranslationKeys.SETTINGS_TWITCH_SEARCH, "Rechercher :" },
                { TranslationKeys.SETTINGS_TWITCH_DO_WELCOME, "Envoyer bienvenue :" },
                { TranslationKeys.SETTINGS_TWITCH_WELCOME_MESSAGE, "Message de bienvenue :" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE_CLAIMED, "Réclamé" },
                { TranslationKeys.SETTINGS_TWITCH_SUB_MODE_ALL, "Tous" },
                { TranslationKeys.DISPLAY_TYPE_TTB, "Vers le bas" },
                { TranslationKeys.DISPLAY_TYPE_RTTB, "Vers le bas (inversé)" },
                { TranslationKeys.DISPLAY_TYPE_BTT, "Vers le haut" },
                { TranslationKeys.DISPLAY_TYPE_RBTT, "Vers le haut (inversé)" },
                { TranslationKeys.ALERT_NAME_INC_RAID, "Raid entrant" },
                { TranslationKeys.ALERT_NAME_OUT_RAID, "Raid sortant" },
                { TranslationKeys.ALERT_NAME_DONATION, "Don" },
                { TranslationKeys.ALERT_NAME_REWARD, "Récompense" },
                { TranslationKeys.ALERT_NAME_FOLLOW, "Follow" },
                { TranslationKeys.ALERT_NAME_SUB_TIER1, "Tier 1" },
                { TranslationKeys.ALERT_NAME_SUB_TIER2, "Tier 2" },
                { TranslationKeys.ALERT_NAME_SUB_TIER3, "Tier 3" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER1, "Don tier 1" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER2, "Don tier 2" },
                { TranslationKeys.ALERT_NAME_GIFT_SUB_TIER3, "Don tier 3" },
                { TranslationKeys.ALERT_NAME_PRIME, "Prime" },
                { TranslationKeys.ALERT_NAME_SHOUTOUT, "Shoutout" },
                { TranslationKeys.ALERT_NAME_BEING_SHOUTOUT, "Being shoutout" },
                { TranslationKeys.ALERT_EDITOR_AUDIO_FILE, "Fichier audio:" },
                { TranslationKeys.ALERT_CHAT_MESSAGE, "Message du chat" }
            };
            Translator.AddTranslation(translationFR);
        }

        private void InitSettings()
        {
            m_Core.SetSettings(m_Settings);
            m_StreamChatPanel.SetDisplayType(m_Settings.Chat.DisplayType);
            m_StreamChatPanel.SetContentFontSize(m_Settings.Chat.MessageFontSize);
            m_StreamAlertPanel.SetDisplayType(m_Settings.Alerts.DisplayType);
            m_StreamAlertPanel.SetContentFontSize(m_Settings.Alerts.MessageFontSize);
            m_AlertManager.InitSettings(m_Settings.Alerts);
            m_HeldMessagePanel.SetDisplayType(m_Settings.Moderation.DisplayType);
            m_HeldMessagePanel.SetContentFontSize(m_Settings.Moderation.MessageFontSize);
        }

        private static void InitStringSources()
        {
            StreamGlassContext.CreateStringSource("viewer_count");
            StreamGlassContext.CreateStringSource("last_bits_donor");
            StreamGlassContext.CreateStringSource("last_bits_donation");
            StreamGlassContext.CreateStringSource("top_bits_donor");
            StreamGlassContext.CreateStringSource("top_bits_donation");
            StreamGlassContext.CreateStringSource("last_follow");
            StreamGlassContext.CreateStringSource("last_raider");
            StreamGlassContext.CreateStringSource("last_gifter");
            StreamGlassContext.CreateStringSource("last_nb_gift");
            StreamGlassContext.CreateStringSource("top_gifter");
            StreamGlassContext.CreateStringSource("top_nb_gift");
            StreamGlassContext.CreateStringSource("last_sub");
        }

        protected override void OnLoad()
        {
            string settingsFilePath = GetFilePath("twitch_settings.json");
            if (File.Exists(settingsFilePath))
            {
                Settings? settings = JsonParser.LoadFromFile<Settings>(settingsFilePath);
                if (settings != null)
                    m_Settings = settings;
            }

            InitTranslation();
            InitSettings();
            InitActions();
            InitCanals();
            InitStringSources();
            m_Core.OnPluginLoad();
        }

        protected override void OnInit()
        {
            m_Core.OnPluginInit();
            m_AlertManager.Init();
            if (m_Settings.AutoConnect)
                m_Core.Connect();
            m_HeldMessagePanel.Init();
            m_StreamChatPanel.Init();
            m_StreamAlertPanel.Init();
            ProfileEditor.SetSearchCategoryDelegate(m_Core.SearchCategoryInfo);
        }

        private void InitActions()
        {
            //TODO Add API actions
            StreamGlassActions.AddAction(new TwitchAd(m_Core), true, true, true);
        }

        private static void InitCanals()
        {
            StreamGlassCanals.NewCanal<Message>(Canals.CHAT_MESSAGE);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_JOINED);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.USER_JOINED);
            StreamGlassCanals.NewCanal<DonationEventArgs>(Canals.DONATION);
            StreamGlassCanals.NewCanal<FollowEventArgs>(Canals.FOLLOW);
            StreamGlassCanals.NewCanal<GiftFollowEventArgs>(Canals.GIFT_FOLLOW);
            StreamGlassCanals.NewCanal<RaidEventArgs>(Canals.RAID);
            StreamGlassCanals.NewCanal<RewardEventArgs>(Canals.REWARD);
            StreamGlassCanals.NewCanal<BanEventArgs>(Canals.BAN);
            StreamGlassCanals.NewCanal<Message>(Canals.HELD_MESSAGE);
            StreamGlassCanals.NewCanal<MessageAllowedEventArgs>(Canals.ALLOW_MESSAGE);
            StreamGlassCanals.NewCanal<string>(Canals.HELD_MESSAGE_MODERATED);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_CLEAR_USER);
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_CLEAR_MESSAGE);
            StreamGlassCanals.NewCanal<ShoutoutEventArgs>(Canals.SHOUTOUT);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.BEING_SHOUTOUT);
            StreamGlassCanals.NewCanal(Canals.CHAT_CLEAR);
            StreamGlassCanals.NewCanal(Canals.STREAM_START);
            StreamGlassCanals.NewCanal(Canals.STREAM_STOP);
            StreamGlassCanals.NewCanal<VisualAlert>(Canals.ALERT);
            StreamGlassCanals.NewCanal<bool>(Canals.ALLOW_AUTOMOD);
        }

        public AEndpoint[] GetEndpoints() => [
            new AllMessageEndpoint(),
            new ClearMessageEndpoint(),
            new ModerateAutoModEndpoint()
        ];

        protected override void OnUnload()
        {
            m_Core.Disconnect();
            JsonParser.WriteToFile<Settings>(GetFilePath("twitch_settings.json"), m_Settings);
        }

        public TabItemContent[] GetSettings() => [
            new StreamChatSettingsItem(m_Settings.Chat, m_StreamChatPanel),
            new StreamAlertSettingsItem(m_Settings.Alerts, m_AlertManager, m_StreamAlertPanel),
            new SettingsItem(m_Settings, m_Core)
        ];

        public void Test() => m_Core.Test();

        public Control? GetPanel(string panelID)
        {
            return panelID switch
            {
                "twitch_alert" => m_StreamAlertPanel,
                "twitch_held_message" => m_HeldMessagePanel,
                "twitch_chat" => m_StreamChatPanel,
                _ => null
            };
        }
    }
}
