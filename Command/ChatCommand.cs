using StreamGlass.Twitch.IRC;

namespace StreamGlass.Command
{
    public struct ChatCommand
    {
        private readonly int m_NBArguments = 0;
        private readonly string m_Content;
        private readonly UserMessage.UserType m_UserType;

        public ChatCommand(string content, int nbArguments, UserMessage.UserType userType)
        {
            m_NBArguments = nbArguments;
            m_Content = content;
            m_UserType = userType;
        }

        public bool CanTrigger(int argc, UserMessage.UserType type) => (NBArguments == -1 || argc == NBArguments) && UserType <= type;

        public int NBArguments => m_NBArguments;
        public string Content => m_Content;
        public UserMessage.UserType UserType => m_UserType;
    }
}
