using CorpseLib.Translation;
using CorpseLib.Web.API;
using StreamGlass.Core.Connections;
using System.Globalization;

namespace StreamGlass.Twitch
{
    public class TwitchPlugin() : APlugin("twitch")
    {
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

        protected override void Register()
        {
            ConnectionManager.RegisterConnection(new Connection(ProfileManager, ConnectionManager, Settings));
        }

        protected override void RegisterAPI(API api) { }
    }
}
