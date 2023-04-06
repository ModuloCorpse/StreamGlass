using StreamFeedstock;
using StreamFeedstock.Placeholder;
using StreamGlass.Connections;
using StreamGlass.StreamChat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace StreamGlass.Profile
{
    public class ChatCommand
    {
        private static readonly Dictionary<string, Context.Function> ms_Functions = new();

        public static void AddFunction(string functionName, Context.Function fct) => ms_Functions[functionName] = fct;
        public static void RemoveFunction(string functionName) => ms_Functions.Remove(functionName);

        private readonly string m_Name = "";
        private readonly string[] m_Aliases = Array.Empty<string>();
        private readonly int m_AwaitTime = 0;
        private readonly int m_NbMessage = 0;
        private readonly string m_Content = "";
        private readonly User.Type m_UserType = User.Type.NONE;
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
        public string[] Aliases => m_Aliases;
        public int AwaitTime => m_AwaitTime;
        public int NbMessage => m_NbMessage;
        public string Content => m_Content;
        public User.Type UserType => m_UserType;
        public ReadOnlyCollection<string> Commands => m_Commands.AsReadOnly();
        public bool AutoTrigger => m_AutoTrigger;
        public int AutoTriggerTime => m_AutoTriggerTime;
        public int AutoTriggerDeltaTime => m_AutoTriggerDeltaTime;
        public string[] AutoTriggerArguments => m_AutoTriggerArguments;

        internal ChatCommand(Json json)
        {
            m_Name = json.GetOrDefault("name", "");
            m_Aliases = json.GetList<string>("aliases").ToArray();
            m_AwaitTime = json.GetOrDefault("time", 0);
            m_NbMessage = json.GetOrDefault("messages", 0);
            m_Content = json.GetOrDefault("content", "");
            m_UserType = json.GetOrDefault("user", User.Type.SELF);
            m_Commands = json.GetList<string>("commands");
            //AutoTrigger
            m_AutoTrigger = json.GetOrDefault("auto_trigger", false);
            m_AutoTriggerTime = json.GetOrDefault("auto_trigger_time", 0);
            m_AutoTriggerDeltaTime = json.GetOrDefault("auto_trigger_delta_time", 0);
            m_AutoTriggerArguments = json.GetList<string>("auto_trigger_argv").ToArray();
        }

        public ChatCommand(string name,
            string[] aliases,
            int awaitTime,
            int nbMessage,
            string content,
            User.Type userType,
            List<string> commands,
            bool autoTrigger,
            int autoTriggerTime,
            int autoTriggerDeltaTime,
            string[] autoTriggerArguments)
        {
            m_Name = name;
            m_Aliases = aliases;
            m_AwaitTime = awaitTime;
            m_NbMessage = nbMessage;
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
            if (m_Aliases.Length > 0)
                json.Set("aliases", m_Aliases);
            if (m_AwaitTime != 0)
                json.Set("time", m_AwaitTime);
            if (m_NbMessage != 0)
                json.Set("messages", m_NbMessage);
            if (!string.IsNullOrWhiteSpace(m_Content))
                json.Set("content", m_Content);
            if (m_UserType != User.Type.SELF)
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

        public bool CanTrigger(User.Type type) => m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= (m_AwaitTime * 1000) && m_UserType <= type;
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

        internal void Trigger(string[] arguments, ConnectionManager connectionManager, string channel)
        {
            if (!string.IsNullOrWhiteSpace(m_Content))
            {
                Context context = new();
                context.AddFunctions(ms_Functions);
                for (int i = 1; i < arguments.Length; ++i)
                    context.AddVariable(string.Format("${0}", i), arguments[i]);
                string contentToSend = Converter.Convert(m_Content, context);
                connectionManager.SendMessage(channel, contentToSend);
                CanalManager.Emit(StreamGlassCanals.COMMANDS, new CommandEventArgs(m_Name, arguments));
                Reset();
            }
        }
    }
}
