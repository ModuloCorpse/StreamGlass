using CorpseLib;
using CorpseLib.DataNotation;
using CorpseLib.Json;
using StreamGlass.Core.Controls;
using StreamGlass.Twitch.Alerts;
using static StreamGlass.Twitch.Alerts.AlertManager;

namespace StreamGlass.Twitch
{
    public class Settings
    {
        public class ChatSettings
        {
            public class DataSerializer : ADataSerializer<ChatSettings>
            {
                protected override OperationResult<ChatSettings> Deserialize(DataObject reader)
                {
                    ChatSettings chatSettings = new();
                    if (reader.TryGet("display", out ScrollPanelDisplayType display))
                        chatSettings.DisplayType = display;
                    if (reader.TryGet("font", out double font))
                        chatSettings.MessageFontSize = font;
                    return new(chatSettings);
                }

                protected override void Serialize(ChatSettings obj, DataObject writer)
                {
                    writer["display"] = obj.DisplayType;
                    writer["font"] = obj.MessageFontSize;
                }
            }

            public ScrollPanelDisplayType DisplayType = ScrollPanelDisplayType.TOP_TO_BOTTOM;
            public double MessageFontSize = 14;
        }

        public class AlertsSettings
        {
            public class DataSerializer : ADataSerializer<AlertsSettings>
            {
                protected override OperationResult<AlertsSettings> Deserialize(DataObject reader)
                {
                    AlertsSettings alertsSettings = new();
                    if (reader.TryGet("display", out ScrollPanelDisplayType display))
                        alertsSettings.DisplayType = display;
                    if (reader.TryGet("font", out double font))
                        alertsSettings.MessageFontSize = font;
                    Dictionary<string, AlertSettings> alerts = reader.GetDictionary<AlertSettings>("alerts");
                    foreach (var alert in alerts)
                        alertsSettings.AlertSettings[alert.Key] = alert.Value;
                    return new(alertsSettings);
                }

                protected override void Serialize(AlertsSettings obj, DataObject writer)
                {
                    writer["display"] = obj.DisplayType;
                    writer["font"] = obj.MessageFontSize;
                    writer["alerts"] = obj.AlertSettings;
                }
            }

            public readonly Dictionary<string, AlertSettings> AlertSettings = new()
            {
                { KnownAlerts.INCOMMING_RAID, new(null, "${ExeDir}/Assets/parachute.png", "${e.From.DisplayName} is raiding you with ${e.NbViewers} viewers", string.Empty, true, false) },
                { KnownAlerts.OUTGOING_RAID, new(null, "${ExeDir}/Assets/parachute.png", "You are raiding ${e.To.DisplayName} with ${e.NbViewers} viewers", string.Empty, true, false) },
                { KnownAlerts.DONATION, new(null, "${ExeDir}/Assets/take-my-money.png", "${e.User.DisplayName} as donated ${e.Amount} ${e.Currency}", string.Empty, true, false) },
                { KnownAlerts.REWARD, new(new("${ExeDir}/Assets/alert-sound.wav", string.Empty, TimeSpan.FromSeconds(30)), "${ExeDir}/Assets/chest.png", "${e.From.DisplayName} retrieve ${e.Reward}: ${e.Input}", string.Empty, true, false) },
                { KnownAlerts.FOLLOW, new(null, "${ExeDir}/Assets/hearts.png", "${e.User.DisplayName} is now following you", string.Empty, true, false) },
                { KnownAlerts.SUB_TIER1, new(null, "${ExeDir}/Assets/stars-stack-1.png", "${e.User.DisplayName} as subscribed to you with a tier 1", string.Empty, true, false) },
                { KnownAlerts.GIFT_SUB_TIER1, new(null, "${ExeDir}/Assets/stars-stack-1.png", "${e.User.DisplayName || 'Anonymous'} offered a sub tier 1", string.Empty, true, false) },
                { KnownAlerts.SUB_TIER2, new(null, "${ExeDir}/Assets/stars-stack-2.png", "${e.User.DisplayName} as subscribed to you with a tier 2", string.Empty, true, false) },
                { KnownAlerts.GIFT_SUB_TIER2, new(null, "${ExeDir}/Assets/stars-stack-2.png", "${e.User.DisplayName || 'Anonymous'} offered a sub tier 2", string.Empty, true, false) },
                { KnownAlerts.SUB_TIER3, new(null, "${ExeDir}/Assets/stars-stack-3.png", "${e.User.DisplayName} as subscribed to you with a tier 3", string.Empty, true, false) },
                { KnownAlerts.GIFT_SUB_TIER3, new(null, "${ExeDir}/Assets/stars-stack-3.png", "${e.User.DisplayName || 'Anonymous'} offered a sub tier 3", string.Empty, true, false) },
                { KnownAlerts.PRIME, new(null, "${ExeDir}/Assets/chess-queen.png", "${e.User.DisplayName} as subscribed to you with a prime", string.Empty, true, false) },
                { KnownAlerts.SHOUTOUT, new(null, "${ExeDir}/Assets/megaphone.png", "${e.Moderator.DisplayName} gave a shoutout to ${e.User.DisplayName}", string.Empty, true, false) },
                { KnownAlerts.BEING_SHOUTOUT, new(null, "${ExeDir}/Assets/megaphone.png", "${e.DisplayName} gave you a shoutout", string.Empty, true, false) },
            };
            public ScrollPanelDisplayType DisplayType = ScrollPanelDisplayType.TOP_TO_BOTTOM;
            public double MessageFontSize = 20;
        }

        public class ModerationSettings
        {
            public class DataSerializer : ADataSerializer<ModerationSettings>
            {
                protected override OperationResult<ModerationSettings> Deserialize(DataObject reader)
                {
                    ModerationSettings moderationSettings = new();
                    if (reader.TryGet("display", out ScrollPanelDisplayType display))
                        moderationSettings.DisplayType = display;
                    if (reader.TryGet("font", out double font))
                        moderationSettings.MessageFontSize = font;
                    return new(moderationSettings);
                }

                protected override void Serialize(ModerationSettings obj, DataObject writer)
                {
                    writer["display"] = obj.DisplayType;
                    writer["font"] = obj.MessageFontSize;
                }
            }

            public ScrollPanelDisplayType DisplayType = ScrollPanelDisplayType.TOP_TO_BOTTOM;
            public double MessageFontSize = 14;
        }

        public class DataSerializer : ADataSerializer<Settings>
        {
            protected override OperationResult<Settings> Deserialize(DataObject reader)
            {
                Settings settings = new();
                if (reader.TryGet("auto_connect", out bool? autoConnect))
                    settings.AutoConnect = (autoConnect == true);
                if (reader.TryGet("do_welcome", out bool? doWelcome))
                    settings.DoWelcome = (doWelcome == true);
                if (reader.TryGet("sub_mode", out string? subMode) && subMode != null)
                    settings.SubMode = subMode;
                if (reader.TryGet("welcome_msg", out string? welcomeMsg) && welcomeMsg != null)
                    settings.WelcomeMessage = welcomeMsg;
                if (reader.TryGet("public_key", out string? publicKey) && publicKey != null)
                    settings.PublicKey = publicKey;
                if (reader.TryGet("secret_key", out string? secretKey) && secretKey != null)
                    settings.SecretKey = secretKey;
                if (reader.TryGet("browser", out string? browser) && browser != null)
                    settings.Browser = browser;
                if (reader.TryGet("chat", out ChatSettings? chat) && chat != null)
                    settings.Chat = chat;
                if (reader.TryGet("alerts", out AlertsSettings? alerts) && alerts != null)
                    settings.Alerts = alerts;
                if (reader.TryGet("moderation", out ModerationSettings? moderation) && moderation != null)
                    settings.Moderation = moderation;
                return new(settings);
            }

            protected override void Serialize(Settings obj, DataObject writer)
            {
                writer["auto_connect"] = obj.AutoConnect;
                writer["do_welcome"] = obj.DoWelcome;
                writer["sub_mode"] = obj.SubMode;
                writer["welcome_msg"] = obj.WelcomeMessage;
                writer["public_key"] = obj.PublicKey;
                writer["secret_key"] = obj.SecretKey;
                writer["browser"] = obj.Browser;
                writer["chat"] = obj.Chat;
                writer["alerts"] = obj.Alerts;
                writer["moderation"] = obj.Moderation;
            }
        }

        public ChatSettings Chat = new();
        public AlertsSettings Alerts = new();
        public ModerationSettings Moderation = new();
        public string Browser = string.Empty;
        public string PublicKey = string.Empty;
        public string SecretKey = string.Empty;
        public string WelcomeMessage = "Hello World! I'm a bot connected with StreamGlass!";
        public string SubMode = "all";
        public bool AutoConnect = false;
        public bool DoWelcome = true;
    }
}
