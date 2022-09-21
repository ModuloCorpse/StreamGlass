namespace StreamGlass.Command
{
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

        public long Time => m_Time;
        public int NbMessage => m_NbMessage;
        public string Command => m_Command;

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
}
