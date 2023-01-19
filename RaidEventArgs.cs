namespace StreamGlass
{
    public class RaidEventArgs
    {
        private readonly string m_FromID;
        private readonly string m_From;
        private readonly string m_ToID;
        private readonly string m_To;
        private readonly int m_NbViewers;
        private readonly bool m_IsIncomming;

        public string FromID => m_FromID;
        public string From => m_From;
        public string ToID => m_ToID;
        public string To => m_To;
        public int NbViewers => m_NbViewers;
        public bool IsIncomming => m_IsIncomming;

        public RaidEventArgs(string fromID, string from, string toID, string to, int nbViewers, bool isIncomming)
        {
            m_FromID = fromID;
            m_From = from;
            m_ToID = toID;
            m_To = to;
            m_NbViewers = nbViewers;
            m_IsIncomming = isIncomming;
        }
    }
}
