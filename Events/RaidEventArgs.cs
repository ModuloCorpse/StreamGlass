using CorpseLib.Json;
using CorpseLib;
using TwitchCorpse;

namespace StreamGlass.Events
{
    public class RaidEventArgs
    {
        public class JSerializer : AJSerializer<RaidEventArgs>
        {
            protected override OperationResult<RaidEventArgs> Deserialize(JObject reader)
            {
                reader.TryGet("from", out TwitchUser? from);
                reader.TryGet("to", out TwitchUser? to);
                if ((from != null || to != null) &&
                    reader.TryGet("nb_viewer", out int? nbViewer) &&
                    reader.TryGet("is_incomming", out bool? isIncomming))
                    return new(new(from, to, (int)nbViewer!, (bool)isIncomming!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(RaidEventArgs obj, JObject writer)
            {
                writer["from"] = obj.m_From;
                writer["to"] = obj.m_To;
                writer["nb_viewer"] = obj.m_NbViewers;
                writer["is_incomming"] = obj.m_IsIncomming;
            }
        }

        private readonly TwitchUser? m_From;
        private readonly TwitchUser? m_To;
        private readonly int m_NbViewers;
        private readonly bool m_IsIncomming;

        public TwitchUser? From => m_From;
        public TwitchUser? To => m_To;
        public int NbViewers => m_NbViewers;
        public bool IsIncomming => m_IsIncomming;

        public RaidEventArgs(TwitchUser? from, TwitchUser? to, int nbViewers, bool isIncomming)
        {
            m_From = from;
            m_To = to;
            m_NbViewers = nbViewers;
            m_IsIncomming = isIncomming;
        }
    }
}
