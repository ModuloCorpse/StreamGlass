namespace StreamGlass
{
    public class RaidEventArgs
    {
        private readonly string m_FromID;
        private readonly string m_From;
        private readonly string m_ToID;
        private readonly string m_To;
        private readonly int m_NbViewers;

        public string FromID => m_FromID;
        public string From => m_From;
        public string ToID => m_ToID;
        public string To => m_To;
        public int NbViewers => m_NbViewers;

        public RaidEventArgs(string fromID, string from, string toID, string to, int nbViewers)
        {
            m_FromID = fromID;
            m_From = from;
            m_ToID = toID;
            m_To = to;
            m_NbViewers = nbViewers;
        }
    }
}
