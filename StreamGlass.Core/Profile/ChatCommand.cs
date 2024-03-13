using CorpseLib.Json;
using CorpseLib.Placeholder;
using System.Collections.ObjectModel;

namespace StreamGlass.Core.Profile
{
    public class ChatCommand
    {
        private readonly string m_Name = string.Empty;
        private readonly string[] m_Aliases = [];
        private readonly int m_AwaitTime = 0;
        private readonly int m_NbMessage = 0;
        private readonly string m_Content = string.Empty;
        private readonly uint m_UserType = 0;
        private readonly List<string> m_Commands = [];
        //Variables use for command that trigger after a certain amount of time
        private readonly bool m_AutoTrigger = false;
        private readonly int m_AutoTriggerTime = 0;
        private readonly int m_AutoTriggerDeltaTime = 0;
        private readonly string[] m_AutoTriggerArguments = [];
        //Variables to check if the command can be triggered
        private long m_TimeSinceLastTrigger = 0;
        private long m_DeltaTime = 0;
        private int m_MessageSinceLastTrigger = 0;

        public string Name => m_Name;
        public string[] Aliases => m_Aliases;
        public int AwaitTime => m_AwaitTime;
        public int NbMessage => m_NbMessage;
        public string Content => m_Content;
        public uint UserType => m_UserType;
        public ReadOnlyCollection<string> Commands => m_Commands.AsReadOnly();
        public bool AutoTrigger => m_AutoTrigger;
        public int AutoTriggerTime => m_AutoTriggerTime;
        public int AutoTriggerDeltaTime => m_AutoTriggerDeltaTime;
        public string[] AutoTriggerArguments => m_AutoTriggerArguments;

        internal ChatCommand(JsonObject json)
        {
            m_Name = json.GetOrDefault("name", string.Empty)!;
            m_Aliases = [.. json.GetList<string>("aliases")];
            m_AwaitTime = json.GetOrDefault("time", 0);
            m_NbMessage = json.GetOrDefault("messages", 0);
            m_Content = json.GetOrDefault("content", string.Empty)!;
            m_UserType = json.GetOrDefault("user", uint.MaxValue);
            m_Commands = json.GetList<string>("commands");
            //AutoTrigger
            m_AutoTrigger = json.GetOrDefault("auto_trigger", false);
            m_AutoTriggerTime = json.GetOrDefault("auto_trigger_time", 0);
            m_AutoTriggerDeltaTime = json.GetOrDefault("auto_trigger_delta_time", 0);
            m_AutoTriggerArguments = [.. json.GetList<string>("auto_trigger_argv")];
        }

        public ChatCommand(string name,
            string[] aliases,
            int awaitTime,
            int nbMessage,
            string content,
            uint userType,
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

        internal JsonObject Serialize()
        {
            JsonObject json = [];
            if (!string.IsNullOrWhiteSpace(m_Name))
                json.Add("name", m_Name);
            if (m_Aliases.Length > 0)
                json.Add("aliases", m_Aliases);
            if (m_AwaitTime != 0)
                json.Add("time", m_AwaitTime);
            if (m_NbMessage != 0)
                json.Add("messages", m_NbMessage);
            if (!string.IsNullOrWhiteSpace(m_Content))
                json.Add("content", m_Content);
            if (m_UserType != uint.MaxValue)
                json.Add("user", m_UserType);
            if (m_Commands.Count != 0)
                json.Add("commands", m_Commands);
            //AutoTrigger
            if (m_AutoTrigger)
                json.Add("auto_trigger", m_AutoTrigger);
            if (m_AutoTriggerTime != 0)
                json.Add("auto_trigger_time", m_AutoTriggerTime);
            if (m_AutoTriggerDeltaTime != 0)
                json.Add("auto_trigger_delta_time", m_AutoTriggerDeltaTime);
            if (m_AutoTriggerArguments.Length != 0)
                json.Add("auto_trigger_argv", m_AutoTriggerArguments);
            return json;
        }

        private void RandomizeDeltaTime()
        {
            if (m_AutoTriggerDeltaTime > 0)
                m_DeltaTime = new Random().Next(-m_AutoTriggerDeltaTime, m_AutoTriggerDeltaTime) * 1000;
            else if (m_AutoTriggerDeltaTime == 0)
                m_DeltaTime = 0;
            else
                m_DeltaTime = new Random().Next(m_AutoTriggerDeltaTime, -m_AutoTriggerDeltaTime) * 1000;
        }

        public bool CanTrigger(uint type) => m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= (m_AwaitTime * 1000) && m_UserType <= type;
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

        internal void Trigger(string[] arguments)
        {
            if (!string.IsNullOrWhiteSpace(m_Content))
            {
                StreamGlassContext context = new();
                for (int i = 1; i < arguments.Length; ++i)
                    context.AddVariable("$" + i.ToString(), arguments[i]);
                string contentToSend = Converter.Convert(m_Content, context);
                StreamGlassCanals.Emit(StreamGlassCanals.SEND_MESSAGE, contentToSend);
                StreamGlassCanals.Emit(StreamGlassCanals.PROFILE_COMMANDS, new ProfileCommandEventArgs(m_Name, arguments));
                Reset();
            }
        }
    }
}
