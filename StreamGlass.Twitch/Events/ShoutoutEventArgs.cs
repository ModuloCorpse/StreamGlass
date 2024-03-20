using CorpseLib;
using CorpseLib.Json;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class ShoutoutEventArgs(TwitchUser moderator, TwitchUser user)
    {
        public class JSerializer : AJsonSerializer<ShoutoutEventArgs>
        {
            protected override OperationResult<ShoutoutEventArgs> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("moderator", out TwitchUser? moderator) &&
                    reader.TryGet("user", out TwitchUser? user))
                    return new(new(moderator!, user!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(ShoutoutEventArgs obj, JsonObject writer)
            {
                writer["moderator"] = obj.m_Moderator;
                writer["user"] = obj.m_User;
            }
        }

        private readonly TwitchUser m_Moderator = moderator;
        private readonly TwitchUser m_User = user;

        public TwitchUser Moderator => m_Moderator;
        public TwitchUser User => m_User;
    }
}
