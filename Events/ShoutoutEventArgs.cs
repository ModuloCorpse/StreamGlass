using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class ShoutoutEventArgs(TwitchUser moderator, TwitchUser user)
    {
        public class JSerializer : AJSerializer<ShoutoutEventArgs>
        {
            protected override OperationResult<ShoutoutEventArgs> Deserialize(JObject reader)
            {
                if (reader.TryGet("moderator", out TwitchUser? moderator) &&
                    reader.TryGet("user", out TwitchUser? user))
                    return new(new(moderator!, user!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(ShoutoutEventArgs obj, JObject writer)
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
