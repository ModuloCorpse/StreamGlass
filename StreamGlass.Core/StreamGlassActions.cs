using CorpseLib.Actions;
using CorpseLib.Shell;

namespace StreamGlass.Core
{
    public static class StreamGlassActions
    {
        private static readonly ActionContainer ms_Actions = [];

        public static void AddAction(AStreamGlassAction action)
        {
            if (action.AllowDirectCall)
                ms_Actions.Add(action);
            if (action.AllowCLICall)
                StreamGlassCLI.AddCommand(new CommandAction(action));
            if (action.AllowScriptCall)
            {
                //TODO Register action to visual allowInScript
            }
            if (action.AllowRemoteCall)
            {
                //TODO Register action to allowInRemote
            }
        }

        public static object?[] Call(string action, params object?[] args) => ms_Actions.SafeCall(action, args);
        public static object?[] Call(string action, Dictionary<string, object?> args) => ms_Actions.Call(action, args);
    }
}
