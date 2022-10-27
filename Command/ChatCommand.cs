using Newtonsoft.Json.Linq;
using StreamGlass.StreamChat;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StreamGlass.Command
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

        private readonly bool m_AutoTrigger;
        private readonly string m_Name;
        private readonly long m_AutoTriggerTime;
        private readonly long m_Time;
        private readonly int m_NbMessage;
        private readonly int m_NBArguments;
        private readonly string[] m_DefaultArguments;
        private readonly string m_Content;
        private long m_TimeSinceLastTrigger = 0;
        private int m_MessageSinceLastTrigger = 0;
        private readonly UserMessage.UserType m_UserType;
        private readonly List<string> m_Commands;

        public string[] GetDefaultArguments() => m_DefaultArguments;
        public string GetContent() => m_Content;
        public List<string> GetCommands() => m_Commands;

        internal ChatCommand(JObject obj)
        {
            m_AutoTrigger = JsonHelper.GetOrDefault(obj, "auto_trigger", false);
            m_AutoTriggerTime = JsonHelper.GetOrDefault(obj, "auto_trigger_time", 0);
            m_Name = JsonHelper.GetOrDefault(obj, "name", "");
            m_Time = JsonHelper.GetOrDefault(obj, "time", 0);
            m_NbMessage = JsonHelper.GetOrDefault(obj, "messages", 0);
            m_NBArguments = JsonHelper.GetOrDefault(obj, "argc", -1);
            m_DefaultArguments = JsonHelper.GetList<string>(obj, "argv").ToArray();
            m_Content = JsonHelper.GetOrDefault(obj, "content", "");
            m_UserType = JsonHelper.GetOrDefault(obj, "user", UserMessage.UserType.SELF);
            m_Commands = JsonHelper.GetList<string>(obj, "commands");
        }

        public ChatCommand(bool autoTrigger, string name, long time, int nbMessage, int nBArguments, string[] defaultArguments, string content, UserMessage.UserType userType, List<string> commands)
        {
            m_AutoTrigger = autoTrigger;
            m_Name = name;
            m_Time = time;
            m_NbMessage = nbMessage;
            m_NBArguments = nBArguments;
            m_DefaultArguments = defaultArguments;
            m_Content = content;
            m_UserType = userType;
            m_Commands = commands;
        }

        public JObject Serialize()
        {
            JObject obj = new();
            if (!string.IsNullOrWhiteSpace(m_Name))
                obj["name"] = m_Name;
            if (!string.IsNullOrWhiteSpace(m_Content))
                obj["content"] = m_Content;
            if (m_UserType != UserMessage.UserType.SELF)
                obj["user"] = (int)m_UserType;
            if (m_NBArguments != -1)
                obj["argc"] = m_NBArguments;
            if (m_NBArguments != -1)
                obj["argv"] = JsonHelper.ToArray(m_DefaultArguments);
            if (m_Time != 0)
                obj["time"] = m_Time;
            if (m_AutoTriggerTime != 0)
                obj["auto_trigger_time"] = m_AutoTriggerTime;
            if (m_NbMessage != 0)
                obj["messages"] = m_NbMessage;
            if (m_AutoTrigger)
                obj["auto_trigger"] = m_AutoTrigger;
            return obj;
        }

        public bool CanTrigger(int argc, UserMessage.UserType type) => (m_NBArguments == -1 || argc == m_NBArguments) && m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= m_Time && m_UserType <= type;
        public bool CanAutoTrigger() => m_AutoTrigger && m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= m_AutoTriggerTime;

        internal void Update(long elapsedTime, int nbMessage)
        {
            m_TimeSinceLastTrigger += elapsedTime;
            m_MessageSinceLastTrigger += nbMessage;
        }

        internal void Reset()
        {
            m_TimeSinceLastTrigger = 0;
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

        internal void Trigger(string[] arguments, IStreamChat streamChat, string channel)
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
                streamChat.SendMessage(channel, contentToSend);
                Reset();
            }
        }
    }
}
