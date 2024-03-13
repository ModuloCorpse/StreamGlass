﻿using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Twitch.Events
{
    public class BanEventArgs
    {
        public class JSerializer : AJsonSerializer<BanEventArgs>
        {
            protected override OperationResult<BanEventArgs> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("user", out TwitchUser? user) &&
                    reader.TryGet("reason", out string? reason) &&
                    reader.TryGet("delay", out uint? delay))
                    return new(new(user!, reason!, (uint)delay!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(BanEventArgs obj, JsonObject writer)
            {
                writer["user"] = obj.m_User;
                writer["reason"] = obj.m_Reason;
                writer["delay"] = obj.m_Delay;
            }
        }

        private readonly TwitchUser m_User;
        private readonly string m_Reason;
        private readonly uint m_Delay;

        public TwitchUser User => m_User;
        public string Reason => m_Reason;
        public uint Delay => m_Delay;

        public BanEventArgs(TwitchUser user, string reason, uint delay)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = delay;
        }

        public BanEventArgs(TwitchUser user, string reason)
        {
            m_User = user;
            m_Reason = reason;
            m_Delay = 0;
        }
    }
}
