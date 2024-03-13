using CorpseLib.Json;
using CorpseLib;

namespace StreamGlass.Core.Profile
{
    public class ProfileCommandEventArgs(string command, string[] arguments)
    {
        public class JSerializer : AJsonSerializer<ProfileCommandEventArgs>
        {
            protected override OperationResult<ProfileCommandEventArgs> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("command", out string? command))
                    return new(new(command!, [.. reader.GetList<string>("arguments")]));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(ProfileCommandEventArgs obj, JsonObject writer)
            {
                writer["arguments"] = obj.m_Arguments;
                writer["command"] = obj.m_Command;
            }
        }

        private readonly string[] m_Arguments = arguments;
        private readonly string m_Command = command;

        public string Command => m_Command;
        public string[] Arguments => m_Arguments;
    }
}
