using CorpseLib;
using CorpseLib.Shell;

namespace StreamGlass.Core
{
    public static class StreamGlassCLI
    {
        private static readonly CLI ms_CLI = new();

        static StreamGlassCLI()
        {
            ms_CLI.Add(new HelpCommand("Help", ms_CLI));
        }

        public static bool AddCommand(Command command) => ms_CLI.Add(command);

        public static string ExecuteCommand(string command)
        {
            OperationResult<string> commandResult = ms_CLI.Execute(command);
            return (commandResult) ? commandResult.Result ?? string.Empty : string.Empty;
        }
    }
}
