using Newtonsoft.Json.Linq;
using StreamGlass.Twitch;
using StreamGlass.Twitch.IRC;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StreamGlass.Command
{
    public class Profile: ManagedObject<Profile>
    {
        public delegate string CommandFunction(string[] args, APICache cache);
        private static readonly Dictionary<string, CommandFunction> ms_Functions = new();

        public static void Init()
        {
            ms_Functions["Game"] = (variables, cache) => {
                var channelInfo = API.GetChannelInfoFromLogin(variables[0], cache);
                if (channelInfo != null)
                    return channelInfo.GameName;
                return variables[0];
            };
            ms_Functions["DisplayName"] = (variables, cache) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0], cache);
                if (userInfo != null)
                    return userInfo.DisplayName;
                return variables[0];
            };
            ms_Functions["Channel"] = (variables, cache) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0], cache);
                if (userInfo != null)
                    return userInfo.Login;
                return variables[0];
            };
            ms_Functions["Lower"] = (variables, _) => variables[0].ToLower();
            ms_Functions["Upper"] = (variables, _) => variables[0].ToUpper();
        }

        public static void AddFunction(string functionName, CommandFunction fct) => ms_Functions[functionName] = fct;

        private string m_Channel = "";
        private readonly Dictionary<string, ChatCommand> m_Commands = new();
        private readonly List<TimedCommand> m_TimedCommands = new();

        public Profile(string name): base(name) {}
        internal Profile(JObject json): base(json) {}

        internal void SetChannel(string channel) => m_Channel = channel;

        public void AddCommand(string command, string message, UserMessage.UserType userType = UserMessage.UserType.NONE, int nbArguments = -1) => m_Commands[command] = new(message, nbArguments, userType);

        public void AddTimedCommand(long time, int nbMessage, string command) => m_TimedCommands.Add(new(time, nbMessage, command));

        private static string TreatVariable(string content, Dictionary<string, string> variables, APICache cache)
        {
            if (content[0] == '$')
            {
                if (content.Length > 1 && variables.TryGetValue(content[1..], out var value))
                    return value;
                return content;
            }
            else if (content[0] == '@')
            {
                if (content.Length > 1)
                    return content[1..];
                return content;
            }
            else if (content.Contains('('))
            {
                int pos = content.IndexOf('(');
                string functionName = content[..pos];
                string functionVariables = content[(pos + 1)..content.LastIndexOf(')')];
                string[] arguments = functionVariables.Split(',').Select(str => TreatVariable(str.Trim(), variables, cache)).ToArray();
                if (ms_Functions.TryGetValue(functionName, out var func))
                    return func(arguments, cache);
                return content;
            }
            return content;
        }

        private void TriggerCommand(string command, UserMessage.UserType userType)
        {
            string[] arguments = command.Split(' ');
            if (m_Commands.TryGetValue(arguments[0], out var content))
            {
                if (content.CanTrigger(arguments.Length - 1, userType))
                {
                    APICache localCache = new();
                    HashSet<string> treatedMatches = new();
                    Dictionary<string, string> variables = new();
                    string contentToSend = content.Content;
                    for (int i = 1; i < arguments.Length; ++i)
                        variables[i.ToString()] = arguments[i];
                    foreach (Match match in Regex.Matches(contentToSend, @"\${[^}]*}").Cast<Match>())
                    {
                        if (!treatedMatches.Contains(match.Value))
                        {
                            contentToSend = contentToSend.Replace(match.Value, TreatVariable(match.Value[2..^1], variables, localCache));
                            treatedMatches.Add(match.Value);
                        }
                    }
                    for (int i = 1; i < arguments.Length; ++i)
                        contentToSend = contentToSend.Replace(string.Format("${0}", i), arguments[i]);
                    API.SendIRCMessage(m_Channel, contentToSend);
                }
            }
            else
                Parent?.TriggerCommand(command, userType);
        }

        internal void OnMessage(UserMessage message)
        {
            if (message.Message[0] == '!')
                TriggerCommand(message.Message[1..], message.SenderType);
        }

        internal void Update(long deltaTime, int nbMessage)
        {
            foreach (TimedCommand timedCommand in m_TimedCommands)
            {
                timedCommand.Update(deltaTime, nbMessage);
                if (timedCommand.CanTrigger())
                {
                    TriggerCommand(timedCommand.GetCommand(), UserMessage.UserType.SELF);
                    timedCommand.Reset();
                }
            }
            Parent?.Update(deltaTime, nbMessage);
        }

        internal void Reset()
        {
            foreach (TimedCommand timedCommand in m_TimedCommands)
                timedCommand.Reset();
            Parent?.Reset();
        }

        protected override void Save(ref JObject json)
        {
            JArray chatCommandArray = new();
            foreach (var command in m_Commands)
            {
                chatCommandArray.Add(new JObject()
                {
                    ["command"] = command.Key,
                    ["content"] = command.Value.Content,
                    ["user"] = (int)command.Value.UserType,
                    ["argc"] = command.Value.NBArguments
                });
            }
            json["chat_commands"] = chatCommandArray;

            JArray timedCommandArray = new();
            foreach (var command in m_TimedCommands)
            {
                timedCommandArray.Add(new JObject()
                {
                    ["time"] = command.Time,
                    ["messages"] = command.NbMessage,
                    ["command"] = command.Command
                });
            }
            json["timed_commands"] = timedCommandArray;
        }

        protected override void Load(JObject json)
        {
            JArray? chatCommandArray = (JArray?)json["chat_commands"];
            if (chatCommandArray != null)
            {
                foreach (JObject obj in chatCommandArray.Cast<JObject>())
                {
                    string? commandName = (string?)obj["command"];
                    string? content = (string?)obj["content"];
                    int? user = (int?)obj["user"];
                    int? argc = (int?)obj["argc"];
                    if (commandName != null && content != null && user != null && argc != null)
                        AddCommand(commandName, content, (UserMessage.UserType)user, (int)argc);
                }
            }
            JArray? timedCommandArray = (JArray?)json["timed_commands"];
            if (timedCommandArray != null)
            {
                foreach (JObject obj in timedCommandArray.Cast<JObject>())
                {
                    int? time = (int?)obj["time"];
                    int? messages = (int?)obj["messages"];
                    string? command = (string?)obj["command"];
                    if (time != null && messages != null && command != null)
                        AddTimedCommand((int)time, (int)messages, command);
                }
            }
        }
    }
}
