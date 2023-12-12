using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class ShoutoutEventArgs
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

        private readonly TwitchUser m_Moderator;
        private readonly TwitchUser m_User;

        public TwitchUser Moderator => m_Moderator;
        public TwitchUser User => m_User;

        public ShoutoutEventArgs(TwitchUser moderator, TwitchUser user)
        {
            m_Moderator = moderator;
            m_User = user;
        }
    }
}
