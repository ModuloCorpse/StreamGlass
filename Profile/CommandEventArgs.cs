using CorpseLib.Json;
using CorpseLib;

namespace StreamGlass.Profile
{
    public class CommandEventArgs
    {
        public class JSerializer : AJSerializer<CommandEventArgs>
        {
            protected override OperationResult<CommandEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("command", out string? command))
                    return new(new(command!, reader.GetList<string>("arguments").ToArray()));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(CommandEventArgs obj, JObject writer)
            {
                writer["arguments"] = obj.m_Arguments;
                writer["command"] = obj.m_Command;
            }
        }

        private readonly string[] m_Arguments;
        private readonly string m_Command;

        public string Command => m_Command;
        public string[] Arguments => m_Arguments;

        public CommandEventArgs(string command, string[] arguments)
        {
            m_Command = command;
            m_Arguments = arguments;
        }
    }
}
