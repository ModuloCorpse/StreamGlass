namespace StreamGlass.Core.Profile
{
    public class TimedCommand(long time, int nbMessage, string command)
    {
        private readonly string m_Command = command;
        private readonly long m_Time = time;
        private long m_TimeSinceLastTrigger = 0;
        private readonly int m_NbMessage = nbMessage;
        private int m_MessageSinceLastTrigger = 0;

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
