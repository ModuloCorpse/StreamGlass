using CorpseLib;
using CorpseLib.DataNotation;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class ShoutoutEventArgs(TwitchUser moderator, TwitchUser user)
    {
        public class DataSerializer : ADataSerializer<ShoutoutEventArgs>
        {
            protected override OperationResult<ShoutoutEventArgs> Deserialize(DataObject reader)
            {
                if (reader.TryGet("moderator", out TwitchUser? moderator) &&
                    reader.TryGet("user", out TwitchUser? user))
                    return new(new(moderator!, user!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(ShoutoutEventArgs obj, DataObject writer)
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
