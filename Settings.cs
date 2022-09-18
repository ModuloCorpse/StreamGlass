using Newtonsoft.Json.Linq;

namespace StreamGlass
{
    public class Settings
    {
        public class TwitchSettings
        {
            public string BotPublic = "";
            public string BotSecret = "";
            public string Channel = "";
            public string BotName = "";
            public string BotToken = "";

            internal JObject ToJson() => new() {
                ["public"] = BotPublic,
                ["secret"] = BotSecret,
                ["channel"] = Channel,
                ["name"] = BotName,
                ["token"] = BotToken
            };

            internal void FromJson(JObject json)
            {
                string? botPublic = (string?)json["public"];
                if (botPublic != null)
                    BotPublic = botPublic;
                string? botSecret = (string?)json["secret"];
                if (botSecret != null)
                    BotSecret = botSecret;
                string? channel = (string?)json["channel"];
                if (channel != null)
                    Channel = channel;
                string? name = (string?)json["name"];
                if (name != null)
                    BotName = name;
                string? token = (string?)json["token"];
                if (token != null)
                    BotToken = token;
            }
        }

        public class DiscordSettings
        {
            public string BotPublic = "";
            public string BotSecret = "";

            internal JObject ToJson() => new() { ["public"] = BotPublic, ["secret"] = BotSecret };

            internal void FromJson(JObject json)
            {
                string? botPublic = (string?)json["public"];
                if (botPublic != null)
                    BotPublic = botPublic;
                string? botSecret = (string?)json["secret"];
                if (botSecret != null)
                    BotSecret = botSecret;
            }
        }

        public TwitchSettings Twitch = new();
        public DiscordSettings Discord = new();

        public void Load()
        {
            if (File.Exists("settings.json"))
            {
                JObject response = JObject.Parse(File.ReadAllText("settings.json"));
                JObject? twitch = (JObject?)response["twitch"];
                if (twitch != null)
                    Twitch.FromJson(twitch);
                JObject? discord = (JObject?)response["discord"];
                if (discord != null)
                    Discord.FromJson(discord);
            }
        }

        public void Save()
        {
            JObject json = new() { ["twitch"] = Twitch.ToJson(), ["discord"] = Discord.ToJson() };
            File.WriteAllText("settings.json", json.ToString());
        }
    }
}
