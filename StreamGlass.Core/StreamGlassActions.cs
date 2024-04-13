using CorpseLib.Actions;
using CorpseLib.Shell;

namespace StreamGlass.Core
{
    public static class StreamGlassActions
    {
        public static void AddAction(AAction action, bool cli, bool script, bool remote)
        {
            if (cli)
                StreamGlassCLI.AddCommand(new CommandAction(action));
            if (script)
            {
                //TODO Register action to visual script
            }
            if (remote)
            {
                //TODO Register action to remote
            }

        }
    }
}
