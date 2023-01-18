namespace StreamGlass.Profile
{
    public class TimedCommand
    {
        private readonly string m_Command;
        private readonly long m_Time;
        private long m_TimeSinceLastTrigger = 0;
        private readonly int m_NbMessage;
        private int m_MessageSinceLastTrigger = 0;

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
