using CorpseLib;
using CorpseLib.DataNotation;
using StreamGlass.Core.Controls;

namespace StreamGlass.Core.Settings
{
    public class BaseSettings
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

        /*public class AlertsSettings
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
        }*/

        public class DataSerializer : ADataSerializer<BaseSettings>
        {
            protected override OperationResult<BaseSettings> Deserialize(DataObject reader)
            {
                BaseSettings settings = new();
                if (reader.TryGet("chat", out ChatSettings? chat) && chat != null)
                    settings.Chat = chat;
                //if (reader.TryGet("alerts", out AlertsSettings? alerts) && alerts != null)
                //    settings.Alerts = alerts;
                return new(settings);
            }

            protected override void Serialize(BaseSettings obj, DataObject writer)
            {
                writer["chat"] = obj.Chat;
                //writer["alerts"] = obj.Alerts;
            }
        }

        public ChatSettings Chat = new();
        //public AlertsSettings Alerts = new();
    }
}
