using System;
using System.Collections.Generic;
using System.Text;

namespace StreamGlass.Twitch.IRC
{
    public class Message
    {
        public class Command
        {
            private readonly string m_Name;
            private readonly string m_Channel;
            private readonly bool m_IsCapRequestEnabled;
            private string m_BotCommand = "";
            private string m_BotCommandParams = "";

            public Command(string name, string channel = "", bool isCapRequestEnabled = false)
            {
                m_Name = name;
                m_Channel = channel;
                m_IsCapRequestEnabled = isCapRequestEnabled;
            }

            internal void SetBotCommand(string command, string param)
            {
                m_BotCommand = command;
                m_BotCommandParams = param;
            }

            public string GetName() => m_Name;
            public string GetChannel() => m_Channel;
            public bool IsCapRequestEnabled() => m_IsCapRequestEnabled;
            public string GetBotCommand() => m_BotCommand;
            public string GetBotCommandParameters() => m_BotCommandParams;
        }

        public class Emote
        {
            private readonly int m_ID;
            private readonly List<Tuple<int, int>> m_Locations = new();

            public Emote(int iD) => m_ID = iD;

            internal void AddLocation(int start, int end) => m_Locations.Add(new(start, end));

            public int GetID() => m_ID;
            public List<Tuple<int, int>> GetLocations() => m_Locations;
            public bool HaveLocations() => m_Locations.Count != 0;

            internal string ToTagStr()
            {
                StringBuilder builder = new();
                builder.Append(m_ID);
                builder.Append(':');
                int i = 0;
                foreach (Tuple<int, int> location in m_Locations)
                {
                    if (i != 0)
                        builder.Append(',');
                    builder.Append(location.Item1);
                    builder.Append('-');
                    builder.Append(location.Item2);
                    ++i;
                }
                return builder.ToString();
            }
        }

        public class Tags
        {
            private readonly Dictionary<string, string> m_Tags = new();
            private readonly Dictionary<string, string> m_Badges = new();
            private readonly Dictionary<string, string> m_BadgeInfos = new();
            private readonly Dictionary<int, Emote> m_Emotes = new();

            internal bool HaveTag(string tag) => m_Tags.ContainsKey(tag);
            internal bool HaveBadge(string badge) => m_Badges.ContainsKey(badge);

            internal string GetTag(string tag)
            {
                if (m_Tags.TryGetValue(tag, out var value))
                    return value;
                return "";
            }

            internal void AddTag(string tag, string value) => m_Tags[tag] = value;
            internal void AddBadge(string badge, string value) => m_Badges[badge] = value;
            internal void AddBadgeInfo(string badge, string info) => m_BadgeInfos[badge] = info;
            internal void AddEmote(int id) => m_Emotes.TryAdd(id, new Emote(id));
            internal void AddEmoteLocation(int id, int start, int end)
            {
                if (m_Emotes.TryGetValue(id, out Emote? emote))
                    emote.AddLocation(start, end);
            }
            internal string ToTagStr()
            {
                StringBuilder builder = new();
                builder.Append('@');
                builder.Append("badge-info=");
                int i = 0;
                foreach (var badgeInfo in m_BadgeInfos)
                {
                    if (i != 0)
                        builder.Append(',');
                    builder.Append(badgeInfo.Key);
                    builder.Append('/');
                    builder.Append(badgeInfo.Value);
                    ++i;
                }
                builder.Append("badges=");
                i = 0;
                foreach (var badge in m_Badges)
                {
                    if (i != 0)
                        builder.Append(',');
                    builder.Append(badge.Key);
                    builder.Append('/');
                    builder.Append(badge.Value);
                    ++i;
                }
                foreach (var tag in m_Tags)
                {
                    builder.Append(tag.Key);
                    builder.Append('=');
                    builder.Append(tag.Value);
                    builder.Append(';');
                }
                if (m_Emotes.Count != 0)
                {

                }
                return builder.ToString();
            }
        }

        private readonly Command m_Command;
        private readonly Tags? m_Tags;
        private readonly string m_Nick;
        private readonly string m_Host;
        private readonly string m_Parameters;

        public Message(string command, string channel = "", string parameters = "", Tags? tags = null)
        {
            m_Command = new(command, channel);
            m_Tags = tags;
            m_Nick = "";
            m_Host = "";
            m_Parameters = parameters;
        }

        internal Message(Command command, Tags? tags, string nick, string host, string parameters)
        {
            m_Command = command;
            m_Tags = tags;
            m_Nick = nick;
            m_Host = host;
            m_Parameters = parameters;
        }

        public override string ToString()
        {
            StringBuilder builder = new();
            if (m_Tags != null)
            {
                builder.Append(m_Tags.ToTagStr());
                builder.Append(' ');
            }
            builder.Append(m_Command.GetName());
            if (!string.IsNullOrWhiteSpace(m_Command.GetChannel()))
            {
                builder.Append(' ');
                builder.Append(m_Command.GetChannel());
            }
            if (!string.IsNullOrWhiteSpace(m_Parameters))
            {
                builder.Append(' ');
                builder.Append(':');
                builder.Append(m_Parameters);
            }
            builder.Append("\r\n");
            return builder.ToString();
        }

        public Command GetCommand() => m_Command;
        public bool HaveTags() => m_Tags != null;
        public Tags? GetTags() => m_Tags;
        public string GetNick() => m_Nick;
        public string GetHost() => m_Host;
        public string GetParameters() => m_Parameters;

        public bool HaveTag(string tag)
        {
            if (m_Tags != null)
                return m_Tags.HaveTag(tag);
            return false;
        }

        public bool HaveBadge(string badge)
        {
            if (m_Tags != null)
                return m_Tags.HaveBadge(badge);
            return false;
        }

        public string GetTag(string tag)
        {
            if (m_Tags != null)
                return m_Tags.GetTag(tag);
            return "";
        }

        internal static Message? Parse(string message)
        {
            string rawTagsComponent = "";
            string rawSourceComponent = "";
            string rawParametersComponent = "";

            if (message[0] == '@')
            {
                int i = message.IndexOf(' ');
                rawTagsComponent = message[1..i];
                message = message[(i + 1)..];
            }

            if (message[0] == ':')
            {
                int i = message.IndexOf(' ');
                rawSourceComponent = message[1..i];
                message = message[(i + 1)..];
            }

            string rawCommandComponent;
            int endIdx = message.IndexOf(':');
            if (endIdx == -1)
                rawCommandComponent = message.Trim();
            else
            {
                rawCommandComponent = message[..endIdx].Trim();
                rawParametersComponent = message[(endIdx + 1)..];
            }

            string[] commandParts = rawCommandComponent.Split(' ');
            Command? command = commandParts[0] switch
            {
                "JOIN" or "PART" or "NOTICE" or "CLEARCHAT" or "HOSTTARGET" or "PRIVMSG" or "USERSTATE" or "ROOMSTATE" => new(commandParts[0], commandParts[1]),
                "PING" or "GLOBALUSERSTATE" or "RECONNECT" => new(commandParts[0]),
                "CAP" => new(commandParts[0], isCapRequestEnabled: (commandParts[2] == "ACK")),
                "421" => new("UNSUPPORTED", commandParts[2]),
                "001" => new("LOGGED"),
                _ => null,
            };
            if (command != null)
            {
                Tags? tags = null;
                string nick = "";
                string host = "";
                if (!string.IsNullOrWhiteSpace(rawTagsComponent))
                {
                    List<string> tagsToIgnore = new() { "client-nonce", "flags" };
                    tags = new();
                    string[] parsedTags = rawTagsComponent.Split(';');
                    foreach (string tag in parsedTags)
                    {
                        string[] parsedTag = tag.Split('=');
                        string tagName = parsedTag[0];
                        string tagValue = parsedTag[1];
                        if (tagName == "badges" || tagName == "badge-info")
                        {
                            if (!string.IsNullOrWhiteSpace(tagValue))
                            {
                                string[] badges = tagValue.Split(',');
                                foreach (string pair in badges)
                                {
                                    string[] badgeParts = pair.Split('/');
                                    if (tagName == "badges")
                                        tags.AddBadge(badgeParts[0], badgeParts[1]);
                                    else
                                        tags.AddBadgeInfo(badgeParts[0], badgeParts[1]);
                                }
                            }
                        }
                        else if (tagName == "emotes")
                        {
                            if (!string.IsNullOrWhiteSpace(tagValue))
                            {
                                Dictionary<string, string> dictEmotes = new();
                                string[] emotes = tagValue.Split('/');
                                foreach (string emote in emotes)
                                {
                                    string[] emoteParts = emote.Split(':');
                                    int emodeID = int.Parse(emoteParts[0]);
                                    string[] positions = emoteParts[1].Split(',');
                                    foreach (string position in positions)
                                    {
                                        string[] positionParts = position.Split('-');
                                        tags.AddEmoteLocation(emodeID, int.Parse(positionParts[0]), int.Parse(positionParts[1]));
                                    }
                                }
                            }
                        }
                        else if (tagName == "emote-sets")
                        {
                            string[] emoteSetIds = tagValue.Split(',');
                            foreach (string emoteSetID in emoteSetIds)
                                tags.AddEmote(int.Parse(emoteSetID));
                        }
                        else
                        {
                            if (!tagsToIgnore.Contains(parsedTag[0]))
                                tags.AddTag(parsedTag[0], parsedTag[1]);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(rawSourceComponent))
                {
                    string[] sourceParts = rawSourceComponent.Split('!');
                    nick = (sourceParts.Length == 2) ? sourceParts[0] : "";
                    host = (sourceParts.Length == 2) ? sourceParts[1] : sourceParts[0];
                }
                if (!string.IsNullOrWhiteSpace(rawParametersComponent) && rawParametersComponent[0] == '!')
                {
                    int idx = 0;
                    string commandParametersParts = rawParametersComponent[(idx + 1)..].Trim();
                    int paramsIdx = commandParametersParts.IndexOf(' ');

                    if (paramsIdx == -1)
                        command.SetBotCommand(commandParametersParts, "");
                    else
                        command.SetBotCommand(commandParametersParts[..paramsIdx], commandParametersParts[paramsIdx..].Trim());
                }
                return new(command, tags, nick, host, rawParametersComponent);
            }
            return null;
        }
    }
}
