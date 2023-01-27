namespace StreamGlass.Events
{
    public class RewardEventArgs
    {
        private readonly string m_FromID;
        private readonly string m_From;
        private readonly string m_Reward;
        private readonly string m_Input;

        public string FromID => m_FromID;
        public string From => m_From;
        public string Reward => m_Reward;
        public string Input => m_Input;

        public RewardEventArgs(string fromID, string from, string reward, string input)
        {
            m_FromID = fromID;
            m_From = from;
            m_Reward = reward;
            m_Input = input;
        }
    }
}
