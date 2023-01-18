using StreamFeedstock;
using StreamGlass.Connections;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace StreamGlass.Profile
{
    public class ChatCommand
    {
        public delegate string CommandFunction(string[] args);
        private static readonly Dictionary<string, CommandFunction> ms_Functions = new();

        public static void Init()
        {
            ms_Functions["Lower"] = (variables) => variables[0].ToLower();
            ms_Functions["Upper"] = (variables) => variables[0].ToUpper();
        }

        public static void AddFunction(string functionName, CommandFunction fct) => ms_Functions[functionName] = fct;
        public static void RemoveFunction(string functionName) => ms_Functions.Remove(functionName);
        private class FunctionResult
        {
            private readonly string[] m_Arguments;
            private readonly string m_Result;

            internal FunctionResult(string[] arguments, string result)
            {
                m_Arguments = arguments;
                m_Result = result;
            }

            internal bool MatchArguments(string[] args)
            {
                if (args.Length != m_Arguments.Length)
                    return false;
                for (int i = 0; i < m_Arguments.Length; ++i)
                {
                    if (m_Arguments[i] != args[i])
                        return false;
                }
                return true;
            }

            internal string Result => m_Result;
        }

        private readonly string m_Name = "";
        private readonly int m_AwaitTime = 0;
        private readonly int m_NbMessage = 0;
        private readonly int m_NBArguments = 0;
        private readonly string m_Content = "";
        private readonly UserMessage.UserType m_UserType = UserMessage.UserType.NONE;
        private readonly List<string> m_Commands = new();
        //Variables use for command that trigger after a certain amount of time
        private readonly bool m_AutoTrigger = false;
        private readonly int m_AutoTriggerTime = 0;
        private readonly int m_AutoTriggerDeltaTime = 0;
        private readonly string[] m_AutoTriggerArguments = Array.Empty<string>();
        //Variables to check if the command can be triggered
        private long m_TimeSinceLastTrigger = 0;
        private long m_DeltaTime = 0;
        private int m_MessageSinceLastTrigger = 0;

        public string Name => m_Name;
        public int AwaitTime => m_AwaitTime;
        public int NbMessage => m_NbMessage;
        public int NBArguments => m_NBArguments;
        public string Content => m_Content;
        public UserMessage.UserType UserType => m_UserType;
        public ReadOnlyCollection<string> Commands => m_Commands.AsReadOnly();
        public bool AutoTrigger => m_AutoTrigger;
        public int AutoTriggerTime => m_AutoTriggerTime;
        public int AutoTriggerDeltaTime => m_AutoTriggerDeltaTime;
        public string[] AutoTriggerArguments => m_AutoTriggerArguments;

        internal ChatCommand(Json json)
        {
            m_Name = json.GetOrDefault("name", "");
            m_AwaitTime = json.GetOrDefault("time", 0);
            m_NbMessage = json.GetOrDefault("messages", 0);
            m_NBArguments = json.GetOrDefault("argc", -1);
            m_Content = json.GetOrDefault("content", "");
            m_UserType = json.GetOrDefault("user", UserMessage.UserType.SELF);
            m_Commands = json.GetList<string>("commands");
            //AutoTrigger
            m_AutoTrigger = json.GetOrDefault("auto_trigger", false);
            m_AutoTriggerTime = json.GetOrDefault("auto_trigger_time", 0);
            m_AutoTriggerDeltaTime = json.GetOrDefault("auto_trigger_delta_time", 0);
            m_AutoTriggerArguments = json.GetList<string>("auto_trigger_argv").ToArray();
        }

        public ChatCommand(string name,
            int awaitTime,
            int nbMessage,
            int nBArguments,
            string content,
            UserMessage.UserType userType,
            List<string> commands,
            bool autoTrigger,
            int autoTriggerTime,
            int autoTriggerDeltaTime,
            string[] autoTriggerArguments)
        {
            m_Name = name;
            m_AwaitTime = awaitTime;
            m_NbMessage = nbMessage;
            m_NBArguments = nBArguments;
            m_Content = content;
            m_UserType = userType;
            m_Commands = commands;
            m_AutoTrigger = autoTrigger;
            m_AutoTriggerTime = autoTriggerTime;
            m_AutoTriggerDeltaTime = autoTriggerDeltaTime;
            m_AutoTriggerArguments = autoTriggerArguments;
            Reset();
        }

        internal Json Serialize()
        {
            Json json = new();
            if (!string.IsNullOrWhiteSpace(m_Name))
                json.Set("name", m_Name);
            if (m_AwaitTime != 0)
                json.Set("time", m_AwaitTime);
            if (m_NbMessage != 0)
                json.Set("messages", m_NbMessage);
            if (m_NBArguments != -1)
                json.Set("argc", m_NBArguments);
            if (!string.IsNullOrWhiteSpace(m_Content))
                json.Set("content", m_Content);
            if (m_UserType != UserMessage.UserType.SELF)
                json.Set("user", m_UserType);
            if (m_Commands.Count != 0)
                json.Set("commands", m_Commands);
            //AutoTrigger
            if (m_AutoTrigger)
                json.Set("auto_trigger", m_AutoTrigger);
            if (m_AutoTriggerTime != 0)
                json.Set("auto_trigger_time", m_AutoTriggerTime);
            if (m_AutoTriggerDeltaTime != 0)
                json.Set("auto_trigger_delta_time", m_AutoTriggerDeltaTime);
            if (m_AutoTriggerArguments.Length != 0)
                json.Set("auto_trigger_argv", m_AutoTriggerArguments);
            return json;
        }

        private void RandomizeDeltaTime()
        {
            if (m_AutoTriggerDeltaTime > 0)
                m_DeltaTime = new Random().Next(-(int)m_AutoTriggerDeltaTime, (int)m_AutoTriggerDeltaTime) * 1000;
            else if (m_AutoTriggerDeltaTime == 0)
                m_DeltaTime = 0;
            else
                m_DeltaTime = new Random().Next((int)m_AutoTriggerDeltaTime, -(int)m_AutoTriggerDeltaTime) * 1000;
        }

        public bool CanTrigger(int argc, UserMessage.UserType type) => (m_NBArguments == -1 || argc == m_NBArguments) && m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= (m_AwaitTime * 1000) && m_UserType <= type;
        public bool CanAutoTrigger() => m_AutoTrigger && m_MessageSinceLastTrigger >= m_NbMessage && (m_TimeSinceLastTrigger + m_DeltaTime) >= (m_AutoTriggerTime * 1000);

        internal void Update(long elapsedTime, int nbMessage)
        {
            m_TimeSinceLastTrigger += elapsedTime;
            m_MessageSinceLastTrigger += nbMessage;
        }

        internal void Reset()
        {
            m_TimeSinceLastTrigger = 0;
            RandomizeDeltaTime();
            m_MessageSinceLastTrigger = 0;
        }

        private static string TreatVariable(string content, ref Dictionary<string, List<FunctionResult>> fctResults)
        {
            if (content[0] == '@')
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
                List<string> argumentsList = new();
                foreach (string functionVariable in functionVariables.Split(','))
                    argumentsList.Add(TreatVariable(functionVariable.Trim(), ref fctResults));
                string[] arguments = argumentsList.ToArray();
                if (fctResults.TryGetValue(functionName, out var fctResult))
                {
                    foreach (FunctionResult functionResult in fctResult)
                    {
                        if (functionResult.MatchArguments(arguments))
                            return functionResult.Result;
                    }
                }
                if (ms_Functions.TryGetValue(functionName, out var func))
                {
                    string ret = func(arguments);
                    if (!fctResults.ContainsKey(functionName))
                        fctResults[functionName] = new();
                    fctResults[functionName].Add(new(arguments, ret));
                    return ret;
                }
                return content;
            }
            return content;
        }

        internal void Trigger(string[] arguments, ConnectionManager connectionManager, string channel)
        {
            string contentToSend = m_Content;
            if (!string.IsNullOrWhiteSpace(contentToSend))
            {
                HashSet<string> treatedMatches = new();
                Dictionary<string, List<FunctionResult>> results = new();
                for (int i = 1; i < arguments.Length; ++i)
                    contentToSend = contentToSend.Replace(string.Format("${0}", i), arguments[i]);
                foreach (Match match in Regex.Matches(contentToSend, @"\${[^}]*}").Cast<Match>())
                {
                    if (!treatedMatches.Contains(match.Value))
                    {
                        contentToSend = contentToSend.Replace(match.Value, TreatVariable(match.Value[2..^1], ref results));
                        treatedMatches.Add(match.Value);
                    }
                }
                connectionManager.SendMessage(channel, contentToSend);
                CanalManager.Emit(StreamGlassCanals.COMMANDS, new CommandEventArgs(m_Name, arguments));
                Reset();
            }
        }
    }
}
