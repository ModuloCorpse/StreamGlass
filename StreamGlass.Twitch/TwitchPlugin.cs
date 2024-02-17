using CorpseLib.Json;
using CorpseLib.Translation;
using CorpseLib.Web.API;
using StreamGlass.Core;
using StreamGlass.Core.Connections;
using StreamGlass.Twitch.API.Message;
using StreamGlass.Twitch.API.Timer;
using StreamGlass.Twitch.Events;
using System.Globalization;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class TwitchPlugin() : APlugin("twitch")
    {
        static TwitchPlugin()
        {
            JHelper.RegisterSerializer(new BanEventArgs.JSerializer());
            JHelper.RegisterSerializer(new DonationEventArgs.JSerializer());
            JHelper.RegisterSerializer(new FollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new GiftFollowEventArgs.JSerializer());
            JHelper.RegisterSerializer(new MessageAllowedEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RaidEventArgs.JSerializer());
            JHelper.RegisterSerializer(new RewardEventArgs.JSerializer());
            JHelper.RegisterSerializer(new ShoutoutEventArgs.JSerializer());
        }

        protected override void InitTranslation()
        {
            Translation translation = new(new CultureInfo("en-US"), true)
            {
                { "settings_twitch_autoconnect", "Auto-Connect:" },
                { "settings_twitch_connect", "Connect" },
                { "settings_twitch_browser", "Browser:" },
                { "settings_twitch_channel", "Twitch channel:" },
                { "settings_twitch_bot_public", "Bot Public Key:" },
                { "settings_twitch_bot_private", "Bot Secret Key:" },
                { "settings_twitch_sub_mode", "Sub Mode:" },
                { "settings_twitch_search", "Search:" },
                { "settings_twitch_do_welcome", "Do Welcome:" },
                { "settings_twitch_welcome_message", "Welcome Message:" }
            };
            Translator.AddTranslation(translation);
        }

        protected override void InitSettings()
        {
            CreateSetting("browser", string.Empty);
            CreateSetting("public_key", string.Empty);
            CreateSetting("secret_key", string.Empty);
            CreateSetting("sub_mode", "all");
            CreateSetting("do_welcome", "true");
            CreateSetting("welcome_message", "Hello World! Je suis un bot connecté via StreamGlass!");
        }

        protected override void InitPlugin()
        {
        }

        protected override void Register()
        {
            ConnectionManager.RegisterConnection(new Connection(ProfileManager, ConnectionManager, Settings));
        }

        protected override void RegisterAPI(CorpseLib.Web.API.API api)
        {
            api.AddEndpoint(new AllMessageEndpoint());
            api.AddEndpoint(new ClearMessageEndpoint());

            //TODO Move in a dedicated plugin
            api.AddEndpoint(new TimerEndpoint());
        }

        protected override void InitCommands() { }

        protected override void InitCanals()
        {
            StreamGlassCanals.NewCanal<string>("chat_joined");
            StreamGlassCanals.NewCanal<TwitchUser>("user_joined");
            StreamGlassCanals.NewCanal<DonationEventArgs>("donation");
            StreamGlassCanals.NewCanal<FollowEventArgs>("follow");
            StreamGlassCanals.NewCanal<GiftFollowEventArgs>("gift_follow");
            StreamGlassCanals.NewCanal<RaidEventArgs>("raid");
            StreamGlassCanals.NewCanal<RewardEventArgs>("reward");
            StreamGlassCanals.NewCanal<BanEventArgs>("ban");
            StreamGlassCanals.NewCanal<UserMessage>("held_message");
            StreamGlassCanals.NewCanal<MessageAllowedEventArgs>("allow_message");
            StreamGlassCanals.NewCanal<string>("held_message_moderated");
            StreamGlassCanals.NewCanal<string>("chat_clear_user");
            StreamGlassCanals.NewCanal<string>("chat_clear_message");
            StreamGlassCanals.NewCanal<ShoutoutEventArgs>("shoutout");
            StreamGlassCanals.NewCanal<TwitchUser>("being_shoutout");
            StreamGlassCanals.NewCanal<uint>("start_ads");
        }
    }
}
