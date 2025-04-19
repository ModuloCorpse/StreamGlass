using CorpseLib.DataNotation;
using CorpseLib.Placeholder;
using System.Collections.ObjectModel;

namespace StreamGlass.Core.Profile
{
    public class ChatCommand
    {
        private readonly List<string> m_Commands = [];
        private readonly string[] m_Aliases = [];
        private readonly string[] m_AutoTriggerArguments = [];
        private readonly string m_Name = string.Empty;
        private readonly string m_Content = string.Empty;
        private long m_TimeSinceLastTrigger = 0;
        private long m_DeltaTime = 0;
        private readonly uint m_UserType = 0;
        private readonly int m_AwaitTime = 0;
        private readonly int m_NbMessage = 0;
        private readonly int m_AutoTriggerTime = 0;
        private readonly int m_AutoTriggerDeltaTime = 0;
        private int m_MessageSinceLastTrigger = 0;
        private int m_LockCount = 0;
        private readonly bool m_AutoTrigger = false;
        private readonly bool m_SharedMessage = false;
        private bool m_IsEnabled = true;

        public string Name => m_Name;
        public string[] Aliases => m_Aliases;
        public int AwaitTime => m_AwaitTime;
        public int NbMessage => m_NbMessage;
        public string Content => m_Content;
        public uint UserType => m_UserType;
        public ReadOnlyCollection<string> Commands => m_Commands.AsReadOnly();
        public bool AutoTrigger => m_AutoTrigger;
        public bool SharedMessage => m_SharedMessage;
        public int AutoTriggerTime => m_AutoTriggerTime;
        public int AutoTriggerDeltaTime => m_AutoTriggerDeltaTime;
        public string[] AutoTriggerArguments => m_AutoTriggerArguments;

        internal ChatCommand(DataObject obj)
        {
            m_Name = obj.GetOrDefault("name", string.Empty)!;
            m_Aliases = [.. obj.GetList<string>("aliases")];
            m_AwaitTime = obj.GetOrDefault("time", 0);
            m_NbMessage = obj.GetOrDefault("messages", 0);
            m_Content = obj.GetOrDefault("content", string.Empty)!;
            m_UserType = obj.GetOrDefault("user", uint.MaxValue);
            m_Commands = obj.GetList<string>("commands");
            m_IsEnabled = obj.GetOrDefault("enabled", true);
            //AutoTrigger
            m_AutoTrigger = obj.GetOrDefault("auto_trigger", false);
            m_AutoTriggerTime = obj.GetOrDefault("auto_trigger_time", 0);
            m_AutoTriggerDeltaTime = obj.GetOrDefault("auto_trigger_delta_time", 0);
            m_AutoTriggerArguments = [.. obj.GetList<string>("auto_trigger_argv")];
            //SharedMessage
            m_SharedMessage = obj.GetOrDefault("shared_message", false);
        }

        public ChatCommand(string name,
            string[] aliases,
            int awaitTime,
            int nbMessage,
            string content,
            uint userType,
            bool sharedMessage,
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
            m_SharedMessage = sharedMessage;
            Reset();
        }

        internal DataObject Serialize()
        {
            DataObject obj = [];
            if (!string.IsNullOrWhiteSpace(m_Name))
                obj.Add("name", m_Name);
            if (m_Aliases.Length > 0)
                obj.Add("aliases", m_Aliases);
            if (m_AwaitTime != 0)
                obj.Add("time", m_AwaitTime);
            if (m_NbMessage != 0)
                obj.Add("messages", m_NbMessage);
            if (!string.IsNullOrWhiteSpace(m_Content))
                obj.Add("content", m_Content);
            if (m_UserType != uint.MaxValue)
                obj.Add("user", m_UserType);
            if (m_Commands.Count != 0)
                obj.Add("commands", m_Commands);
            obj.Add("enabled", m_IsEnabled);
            //AutoTrigger
            if (m_AutoTrigger)
                obj.Add("auto_trigger", m_AutoTrigger);
            if (m_AutoTriggerTime != 0)
                obj.Add("auto_trigger_time", m_AutoTriggerTime);
            if (m_AutoTriggerDeltaTime != 0)
                obj.Add("auto_trigger_delta_time", m_AutoTriggerDeltaTime);
            if (m_AutoTriggerArguments.Length != 0)
                obj.Add("auto_trigger_argv", m_AutoTriggerArguments);
            if (m_SharedMessage)
                obj.Add("shared_message", m_SharedMessage);
            return obj;
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

        public void Lock() => ++m_LockCount;
        public void Unlock() => --m_LockCount;

        public void Enable() => m_IsEnabled = true;
        public void Disable() => m_IsEnabled = false;
        public void SetEnable(bool enabled) => m_IsEnabled = enabled;

        public bool IsEnabled() => m_LockCount == 0 && m_IsEnabled;
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
                //TODO
                DataObject messageData = new() { { "message", contentToSend }, { "for_source_only", !m_SharedMessage } };
                StreamGlassChat.SendMessage("twitch", messageData);
                StreamGlassCanals.Emit(StreamGlassCanals.PROFILE_COMMANDS, new ProfileCommandEventArgs(m_Name, arguments));
                Reset();
            }
        }
    }
}
