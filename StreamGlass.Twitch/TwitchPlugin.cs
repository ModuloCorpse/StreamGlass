﻿using CorpseLib.DataNotation;
using CorpseLib.Encryption;
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
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class TwitchPlugin() : APlugin("Twitch"), IAPIPlugin, ITestablePlugin, ISettingsPlugin, IPanelPlugin
    {
        public static class VaultKeys
        {
            public static readonly string SECRET = "twitch_secret";
            public static readonly string API_TOKEN = "twitch_api_token";
            public static readonly string IRC_TOKEN = "twitch_irc_token";
        }

        public static class Canals
        {
            public static readonly string CHAT_JOINED = "twitch_chat_joined";
            public static readonly string USER_JOINED = "twitch_user_joined";
            public static readonly string DONATION = "twitch_donation";
            public static readonly string FOLLOW = "twitch_follow";
            public static readonly string GIFT_FOLLOW = "twitch_gift_follow";
            public static readonly string RAID = "twitch_raid";
            public static readonly string REWARD = "twitch_reward";
            public static readonly string HELD_MESSAGE = "twitch_held_message";
            public static readonly string ALLOW_MESSAGE = "twitch_allow_message";
            public static readonly string HELD_MESSAGE_MODERATED = "twitch_held_message_moderated";
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
            public static readonly TranslationKey MENU_SHOUTOUT = new("twitch_menu_shoutout");
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
            public static readonly TranslationKey ALERT_NAME_CHAT_MESSAGE = new("twitch_alert_name_chat_message");
            public static readonly TranslationKey ALERT_EDITOR_AUDIO_FILE = new("twitch_alert_editor_audio_file");
            public static readonly TranslationKey ALERT_CHAT_MESSAGE = new("twitch_alert_chat_message");
        }

        private static readonly Metadata ms_PluginMetadata = Metadata.CreateNativeMetadata<TwitchPlugin>("twitch", "StreamGlass.Twitch.dll");

        public static Metadata PluginMetadata => ms_PluginMetadata;

        static TwitchPlugin()
        {
            DataHelper.RegisterSerializer(new TwitchUser.DataSerializer());
            DataHelper.RegisterSerializer(new TwitchBadgeInfo.DataSerializer());
            DataHelper.RegisterSerializer(new BanEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new DonationEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new FollowEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new GiftFollowEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new MessageAllowedEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new RaidEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new RewardEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new ShoutoutEventArgs.DataSerializer());
            DataHelper.RegisterSerializer(new Message.DataSerializer());
            DataHelper.RegisterSerializer(new VisualAlert.DataSerializer());

            DataHelper.RegisterSerializer(new AlertSettings.DataSerializer());
            DataHelper.RegisterSerializer(new Settings.AlertsSettings.DataSerializer());
            DataHelper.RegisterSerializer(new Settings.ModerationSettings.DataSerializer());
            DataHelper.RegisterSerializer(new Settings.DataSerializer());
        }

        private readonly WindowsEncryptionAlgorithm m_WindowsEncryptionAlgorithm = new([95, 239, 5, 252, 160, 29, 242, 88, 31, 3]);
        private readonly Core m_Core = new();
        private readonly AlertManager m_AlertManager = new();
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
                { TranslationKeys.MENU_BAN, "[Twitch] Ban User" },
                { TranslationKeys.MENU_SHOUTOUT, "[Twitch] Shoutout User" },
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
                { TranslationKeys.ALERT_NAME_CHAT_MESSAGE, "Chat message" },
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
                { TranslationKeys.MENU_BAN, "[Twitch] Bannir l'utilisateur" },
                { TranslationKeys.MENU_SHOUTOUT, "[Twitch] Shoutout l'utilisateur" },
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
                { TranslationKeys.ALERT_NAME_BEING_SHOUTOUT, "Shoutout reçu" },
                { TranslationKeys.ALERT_NAME_CHAT_MESSAGE, "Message dans le chat" },
                { TranslationKeys.ALERT_EDITOR_AUDIO_FILE, "Fichier audio:" },
                { TranslationKeys.ALERT_CHAT_MESSAGE, "Message du chat" }
            };
            Translator.AddTranslation(translationFR);
        }

        private void InitSettings()
        {
            m_Core.SetSettings(m_Settings);
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
            string settingsSaveFilePath = GetFilePath("twitch_settings.json");
            if (File.Exists(settingsSaveFilePath))
            {
                Settings? settings = JsonParser.LoadFromFile<Settings>(settingsSaveFilePath);
                if (settings != null)
                {
                    m_Settings = settings;
                    m_Settings.SecretKey = StreamGlassVault.Load(VaultKeys.SECRET);
                }
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
            m_StreamAlertPanel.Init();
            ProfileEditor.SetSearchCategoryDelegate(m_Core.SearchCategoryInfo);
        }

        private void InitActions()
        {
            //TODO Add API actions
            StreamGlassActions.AddAction(new TwitchAd(m_Core));
        }

        private static void InitCanals()
        {
            StreamGlassCanals.NewCanal<string>(Canals.CHAT_JOINED);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.USER_JOINED);
            StreamGlassCanals.NewCanal<DonationEventArgs>(Canals.DONATION);
            StreamGlassCanals.NewCanal<FollowEventArgs>(Canals.FOLLOW);
            StreamGlassCanals.NewCanal<GiftFollowEventArgs>(Canals.GIFT_FOLLOW);
            StreamGlassCanals.NewCanal<RaidEventArgs>(Canals.RAID);
            StreamGlassCanals.NewCanal<RewardEventArgs>(Canals.REWARD);
            StreamGlassCanals.NewCanal<Message>(Canals.HELD_MESSAGE);
            StreamGlassCanals.NewCanal<MessageAllowedEventArgs>(Canals.ALLOW_MESSAGE);
            StreamGlassCanals.NewCanal<string>(Canals.HELD_MESSAGE_MODERATED);
            StreamGlassCanals.NewCanal<ShoutoutEventArgs>(Canals.SHOUTOUT);
            StreamGlassCanals.NewCanal<TwitchUser>(Canals.BEING_SHOUTOUT);
            StreamGlassCanals.NewCanal(Canals.CHAT_CLEAR);
            StreamGlassCanals.NewCanal(Canals.STREAM_START);
            StreamGlassCanals.NewCanal(Canals.STREAM_STOP);
            StreamGlassCanals.NewCanal<VisualAlert>(Canals.ALERT);
            StreamGlassCanals.NewCanal<bool>(Canals.ALLOW_AUTOMOD);
        }

        public Dictionary<CorpseLib.Web.Http.Path, AEndpoint> GetEndpoints() => new() {
            { new("/clear_chat"), new ClearMessageEndpoint() },
            { new("/automod"), new ModerateAutoModEndpoint() }
        };

        protected override void OnUnload()
        {
            m_Core.Disconnect();

            string settingsFilePath = GetFilePath("twitch_settings.json");
            JsonParser.WriteToFile<Settings>(settingsFilePath, m_Settings);
        }

        public TabItemContent[] GetSettings() => [
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
                _ => null
            };
        }
    }
}
