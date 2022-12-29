namespace StreamGlass.Profile
{
    public class CommandEventArgs
    {
        private readonly string m_Command;
        private readonly string[] m_Arguments;

        public string Command => m_Command;
        public string[] Arguments => m_Arguments;

        public CommandEventArgs(string command, string[] arguments)
        {
            m_Command = command;
            m_Arguments = arguments;
        }
    }
}
