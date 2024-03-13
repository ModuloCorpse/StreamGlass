using CorpseLib.Ini;
using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.Audio;
using StreamGlass.Twitch.Events;
using TwitchCorpse;

namespace StreamGlass.Twitch.Alerts
{
    public class AlertManager
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
        }

        private readonly Dictionary<string, Alert> m_Alerts = [];

        internal Dictionary<string, Alert> Alerts => m_Alerts;

        public AlertManager()
        {
            NewAlert(KnownAlerts.INCOMMING_RAID, TwitchPlugin.TranslationKeys.ALERT_NAME_INC_RAID,
                null, "${ExeDir}/Assets/parachute.png",
                "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers");
            
            NewAlert(KnownAlerts.OUTGOING_RAID, TwitchPlugin.TranslationKeys.ALERT_NAME_OUT_RAID,
                null, "${ExeDir}/Assets/parachute.png",
                "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers");
            
            NewAlert(KnownAlerts.DONATION, TwitchPlugin.TranslationKeys.ALERT_NAME_DONATION,
                null, "${ExeDir}/Assets/take-my-money.png",
                "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}");
            
            NewAlert(KnownAlerts.REWARD, TwitchPlugin.TranslationKeys.ALERT_NAME_REWARD,
                new("${ExeDir}/Assets/alert-sound.wav", string.Empty), "${ExeDir}/Assets/chest.png",
                "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}");
            
            NewAlert(KnownAlerts.FOLLOW, TwitchPlugin.TranslationKeys.ALERT_NAME_FOLLOW,
                null, "${ExeDir}/Assets/hearts.png",
                "${e.User.DisplayName} is now following you");
            
            NewAlert(KnownAlerts.SUB_TIER1, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER1,
                null, "${ExeDir}/Assets/stars-stack-1.png",
                "${e.User.DisplayName} as subscribed to you with a tier 1");
            
            NewAlert(KnownAlerts.GIFT_SUB_TIER1, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER1,
                null, "${ExeDir}/Assets/stars-stack-1.png",
                "${e.User.DisplayName || 'Anonymous'} offered a sub tier 1");
            
            NewAlert(KnownAlerts.SUB_TIER2, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER2,
                null, "${ExeDir}/Assets/stars-stack-2.png",
                "${e.User.DisplayName} as subscribed to you with a tier 2");
            
            NewAlert(KnownAlerts.GIFT_SUB_TIER2, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER2,
                null, "${ExeDir}/Assets/stars-stack-2.png",
                "${e.User.DisplayName || 'Anonymous'} offered a sub tier 2");
            
            NewAlert(KnownAlerts.SUB_TIER3, TwitchPlugin.TranslationKeys.ALERT_NAME_SUB_TIER3,
                null, "${ExeDir}/Assets/stars-stack-3.png",
                "${e.User.DisplayName} as subscribed to you with a tier 3");
            
            NewAlert(KnownAlerts.GIFT_SUB_TIER3, TwitchPlugin.TranslationKeys.ALERT_NAME_GIFT_SUB_TIER3,
                null, "${ExeDir}/Assets/stars-stack-3.png",
                "${e.User.DisplayName || 'Anonymous'} offered a sub tier 3");
            
            NewAlert(KnownAlerts.PRIME, TwitchPlugin.TranslationKeys.ALERT_NAME_PRIME,
                null, "${ExeDir}/Assets/chess-queen.png",
                "${e.User.DisplayName} as subscribed to you with a prime");
            
            NewAlertWithChatMessage(KnownAlerts.SHOUTOUT, TwitchPlugin.TranslationKeys.ALERT_NAME_SHOUTOUT,
                null, "${ExeDir}/Assets/megaphone.png",
                "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", "Go check ${DisplayName(Lower(e.User.DisplayName))}, who's playing ${Game(e.User.DisplayName)} on https://twitch.tv/${e.User.Name}");
            
            NewAlert(KnownAlerts.BEING_SHOUTOUT, TwitchPlugin.TranslationKeys.ALERT_NAME_BEING_SHOUTOUT,
                null, "${ExeDir}/Assets/megaphone.png",
                "${e.DisplayName} gave you a shoutout");
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

        private void NewAlert(string alertID, TranslationKey translationKey, Sound? audio, string imgPath, string prefix)
        {
            m_Alerts[alertID] = new Alert(translationKey, alertID, new(audio, imgPath, prefix, string.Empty, true, false));
        }

        private void NewAlertWithChatMessage(string alertID, TranslationKey translationKey, Sound? audio, string imgPath, string prefix, string chatMessage)
        {
            m_Alerts[alertID] = new Alert(translationKey, alertID, new(audio, imgPath, prefix, chatMessage, true, true));
        }

        internal void InitSettings(IniSection alertSection)
        {
            foreach (var pair in m_Alerts)
                pair.Value.LoadSettings(alertSection);
        }

        private void TriggerAlert(string alertID, object e, Text? message = null)
        {
            if (m_Alerts.TryGetValue(alertID, out Alert? alert))
            {
                StreamGlassContext.LOGGER.Log("Adding visual alert");
                AlertSettings alertSettings = alert.Settings;
                if (alertSettings.IsEnabled)
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
                        StreamGlassCanals.Emit(StreamGlassCanals.SEND_MESSAGE, Converter.Convert(alertSettings.ChatMessage, context));

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
    }
}
