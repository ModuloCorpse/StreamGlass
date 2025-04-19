using CorpseLib.DataNotation;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Audio;
using StreamGlass.Core.StreamChat;
using StreamGlass.Twitch.Events;
using TwitchCorpse;

namespace StreamGlass.Twitch.Alerts
{
    public class AlertManager : IMessageReceiver
    {
        public class KnownAlerts
        {
            public static readonly string INCOMMING_RAID = "incomming_raid";
            public static readonly string OUTGOING_RAID = "outgoing_raid";
            public static readonly string DONATION = "donation";
            public static readonly string REWARD = "reward";
            public static readonly string FOLLOW = "follow";
            public static readonly string SUB_TIER1 = "sub_tier1";
            public static readonly string GIFT_SUB_TIER1 = "sub_tier1_gift";
            public static readonly string SUB_TIER2 = "sub_tier2";
            public static readonly string GIFT_SUB_TIER2 = "sub_tier2_gift";
            public static readonly string SUB_TIER3 = "sub_tier3";
            public static readonly string GIFT_SUB_TIER3 = "sub_tier3_gift";
            public static readonly string PRIME = "prime";
            public static readonly string SHOUTOUT = "shoutout";
            public static readonly string BEING_SHOUTOUT = "being_shoutout";
            public static readonly string CHAT_MESSAGE = "chat_message";
        }

        private readonly Dictionary<string, Alert> m_Alerts = [];

        internal Dictionary<string, Alert> Alerts => m_Alerts;

        public AlertManager()
        {
            NewAlert(KnownAlerts.INCOMMING_RAID, TwitchPlugin.TranslationKeys.ALERT_NAME_INC_RAID);
            NewAlert(KnownAlerts.OUTGOING_RAID, TwitchPlugin.TranslationKeys.ALERT_NAME_OUT_RAID);
            NewAlert(KnownAlerts.DONATION, TwitchPlugin.TranslationKeys.ALERT_NAME_DONATION);
            NewAlert(KnownAlerts.REWARD, TwitchPlugin.TranslationKeys.ALERT_NAME_REWARD);
            NewAlert(KnownAlerts.FOLLOW, TwitchPlugin.TranslationKeys.ALERT_NAME_FOLLOW);
            NewAlert(KnownAlerts.SUB_TIER1, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER1);
            NewAlert(KnownAlerts.GIFT_SUB_TIER1, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER1);
            NewAlert(KnownAlerts.SUB_TIER2, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER2);
            NewAlert(KnownAlerts.GIFT_SUB_TIER2, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER2);
            NewAlert(KnownAlerts.SUB_TIER3, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER3);
            NewAlert(KnownAlerts.GIFT_SUB_TIER3, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER3);
            NewAlert(KnownAlerts.PRIME, TwitchPlugin.TranslationKeys.ALERT_NAME_PRIME);
            NewAlert(KnownAlerts.SHOUTOUT, TwitchPlugin.TranslationKeys.ALERT_NAME_SHOUTOUT);
            NewAlert(KnownAlerts.BEING_SHOUTOUT, TwitchPlugin.TranslationKeys.ALERT_NAME_BEING_SHOUTOUT);
            NewAlert(KnownAlerts.CHAT_MESSAGE, TwitchPlugin.TranslationKeys.ALERT_NAME_CHAT_MESSAGE);
            StreamGlassChat.RegisterMessageReceiver(this);
        }

        public void Init()
        {
            StreamGlassCanals.Register<DonationEventArgs>(TwitchPlugin.Canals.DONATION, OnDonation);
            StreamGlassCanals.Register<FollowEventArgs>(TwitchPlugin.Canals.FOLLOW, OnNewFollow);
            StreamGlassCanals.Register<GiftFollowEventArgs>(TwitchPlugin.Canals.GIFT_FOLLOW, OnNewGiftFollow);
            StreamGlassCanals.Register<RaidEventArgs>(TwitchPlugin.Canals.RAID, OnRaid);
            StreamGlassCanals.Register<RewardEventArgs>(TwitchPlugin.Canals.REWARD, OnReward);
            StreamGlassCanals.Register<ShoutoutEventArgs>(TwitchPlugin.Canals.SHOUTOUT, OnShoutout);
            StreamGlassCanals.Register<TwitchUser>(TwitchPlugin.Canals.BEING_SHOUTOUT, OnBeingShoutout);
        }

        private void NewAlert(string alertID, TranslationKey translationKey) => m_Alerts[alertID] = new Alert(translationKey, alertID);

        internal void InitSettings(Settings.AlertsSettings settings)
        {
            foreach (var pair in m_Alerts)
                pair.Value.LoadSettings(settings);
        }

        private void TriggerAlert(string alertID, object e, Text? message = null)
        {
            if (m_Alerts.TryGetValue(alertID, out Alert? alert))
            {
                StreamGlassContext.LOGGER.Log("Adding visual alert");
                AlertSettings alertSettings = alert.Settings;
                if (alertSettings.IsEnabled)
                {
                    if (alertID != KnownAlerts.CHAT_MESSAGE)
                    {
                        StreamGlassContext context = new();
                        context.AddVariable("e", e);
                        string alertPrefix = Converter.Convert(alertSettings.Prefix, context);
                        Text alertMessage = new(alertPrefix);
                        if (message != null && !message.IsEmpty)
                        {
                            alertMessage.AddText(": ");
                            alertMessage.Append(message);
                        }
                        StreamGlassCanals.Emit<VisualAlert>(TwitchPlugin.Canals.ALERT, new(alertSettings.ImgPath, alertMessage));
                        if (alertSettings.HaveChatMessage)
                        {
                            //TODO
                            DataObject messageData = new() { { "message", Converter.Convert(alertSettings.ChatMessage, context) }, { "for_source_only", true } };
                            StreamGlassChat.SendMessage("twitch", messageData);
                        }
                    }
                    if (alertSettings.Audio != null)
                        SoundManager.PlaySound(alertSettings.Audio);
                }
            }
        }

        private void OnNewFollow(FollowEventArgs? obj)
        {
            FollowEventArgs e = obj!;
            StreamGlassContext.LOGGER.Log("New follow alert");
            switch (e.Tier)
            {
                case 0: TriggerAlert(KnownAlerts.FOLLOW, e, e.Message); break;
                case 1: TriggerAlert(KnownAlerts.SUB_TIER1, e, e.Message); break;
                case 2: TriggerAlert(KnownAlerts.SUB_TIER2, e, e.Message); break;
                case 3: TriggerAlert(KnownAlerts.SUB_TIER3, e, e.Message); break;
                case 4: TriggerAlert(KnownAlerts.PRIME, e, e.Message); break;
            }
        }

        private void OnNewGiftFollow(GiftFollowEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New gift follow alert");
            GiftFollowEventArgs e = obj!;
            switch (e.Tier)
            {
                case 1: TriggerAlert(KnownAlerts.GIFT_SUB_TIER1, e, e.Message); break;
                case 2: TriggerAlert(KnownAlerts.GIFT_SUB_TIER2, e, e.Message); break;
                case 3: TriggerAlert(KnownAlerts.GIFT_SUB_TIER3, e, e.Message); break;
            }
        }

        private void OnDonation(DonationEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New donation alert");
            TriggerAlert(KnownAlerts.DONATION, obj!, (obj!).Message);
        }

        private void OnRaid(RaidEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New raid alert");
            TriggerAlert((obj!).IsIncomming ? KnownAlerts.INCOMMING_RAID : KnownAlerts.OUTGOING_RAID, obj!);
        }

        private void OnReward(RewardEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New reward alert");
            TriggerAlert(KnownAlerts.REWARD, obj!);
        }

        private void OnShoutout(ShoutoutEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New shoutout alert");
            TriggerAlert(KnownAlerts.SHOUTOUT, obj!);
        }

        private void OnBeingShoutout(TwitchUser? obj)
        {
            StreamGlassContext.LOGGER.Log("New received shoutout alert");
            TriggerAlert(KnownAlerts.BEING_SHOUTOUT, obj!);
        }

        public void AddMessage(StreamGlass.Core.StreamChat.Message message)
        {
            if (m_Alerts.TryGetValue(KnownAlerts.CHAT_MESSAGE, out Alert? alert))
            {
                StreamGlassContext.LOGGER.Log("Adding visual alert");
                AlertSettings alertSettings = alert.Settings;
                if (alertSettings.IsEnabled)
                {
                    if (alertSettings.Audio != null)
                        SoundManager.PlaySound(alertSettings.Audio);
                }
            }
        }

        public void RemoveMessages(string[] messageIDs) { }
    }
}
