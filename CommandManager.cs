using StreamGlass.Twitch.IRC;
using System.Text.RegularExpressions;

namespace StreamGlass
{
    public class CommandManager
    {
        public struct Command
        {
            public readonly int m_NBArguments = 0;
            public readonly string Content;
            public readonly UserMessage.UserType UserType;

            public Command(string content, UserMessage.UserType userType)
            {
                Content = content;
                UserType = userType;
                for (int i = 1; Content.Contains("${" + i + "}"); ++i)
                    ++m_NBArguments;
            }
        }

        public class TimedCommand
        {
            private readonly long m_Time;
            private readonly int m_NbMessage;
            private long m_TimeSinceLastTrigger = 0;
            private int m_MessageSinceLastTrigger = 0;
            private readonly string m_Command;

            public TimedCommand(long time, int nbMessage, string command)
            {
                m_Time = time;
                m_NbMessage = nbMessage;
                m_Command = command;
            }

            internal string GetCommand() => m_Command;

            internal void Update(long elapsedTime, int nbMessage)
            {
                m_TimeSinceLastTrigger += elapsedTime;
                m_MessageSinceLastTrigger += nbMessage;
            }

            internal bool CanTrigger() => m_MessageSinceLastTrigger >= m_NbMessage && m_TimeSinceLastTrigger >= m_Time;

            internal void Reset()
            {
                m_TimeSinceLastTrigger = 0;
                m_MessageSinceLastTrigger = 0;
            }
        }

        private delegate string CommandFunction(string[] args);

        private int m_NbMessage = 0;
        private string m_Channel = "";
        private readonly Client m_IRCClient;
        private readonly Dictionary<string, Command> m_Commands = new();
        private readonly Dictionary<string, CommandFunction> m_Functions = new();
        private readonly List<TimedCommand> m_TimedCommands = new();

        public CommandManager(Client iRCClient)
        {
            m_IRCClient = iRCClient;
            m_Functions["Game"] = (variables) => "Forewarned";
            m_Functions["DisplayName"] = (variables) => variables[0];
            m_Functions["Channel"] = (variables) => variables[0];
        }

        internal void SetChannel(string channel) => m_Channel = channel;

        public void AddCommand(string command, string message, UserMessage.UserType userType = UserMessage.UserType.NONE) => m_Commands[command] = new(message, userType);

        public void AddTimedCommand(long time, int nbMessage, string command) => m_TimedCommands.Add(new(time, nbMessage, command));

        private void TriggerCommand(string command, UserMessage.UserType userType)
        {
            //!so capterge => [so capterge] => 0:[so], 1:[capterge]
            string[] arguments = command.Split(' ');
            if (m_Commands.TryGetValue(arguments[0], out var content) &&
                content.m_NBArguments == (arguments.Length - 1) &&
                content.UserType <= userType)
            {
                string contentToSend = content.Content;
                for (int i = 1; i < arguments.Length; ++i)
                    contentToSend = contentToSend.Replace("${" + i + "}", arguments[i]);
                foreach (Match match in Regex.Matches(contentToSend, @"\${[^}]*}"))
                {
                    string varContent = match.Value[2..^1];
                    int pos = varContent.IndexOf('(');
                    string functionName = varContent[..pos];
                    string functionVariables = varContent[(pos + 1)..varContent.IndexOf(')')];
                    string[] variables = functionVariables.Split(',').Select(str => str.Trim()).ToArray();
                    string ret = "";
                    if (m_Functions.TryGetValue(functionName, out var func))
                        ret = func(variables);
                    contentToSend = contentToSend.Replace(match.Value, ret);
                }
                m_IRCClient.SendMessage(m_Channel, contentToSend);
            }
        }

        internal void OnMessage(UserMessage message)
        {
            ++m_NbMessage;
            if (message.Message[0] == '!')
                TriggerCommand(message.Message[1..], message.SenderType);
        }

        internal void Update(long deltaTime)
        {
            foreach (TimedCommand timedCommand in m_TimedCommands)
            {
                timedCommand.Update(deltaTime, m_NbMessage);
                if (timedCommand.CanTrigger())
                {
                    TriggerCommand(timedCommand.GetCommand(), UserMessage.UserType.SELF);
                    timedCommand.Reset();
                }
            }
            m_NbMessage = 0;
        }
    }
}
