using CorpseLib;
using CorpseLib.DataNotation;
using TwitchCorpse.API;

namespace StreamGlass.Twitch.Events
{
    public class RaidEventArgs(TwitchUser? from, TwitchUser? to, int nbViewers, bool isIncomming)
    {
        public class DataSerializer : ADataSerializer<RaidEventArgs>
        {
            protected override OperationResult<RaidEventArgs> Deserialize(DataObject reader)
            {
                reader.TryGet("from", out TwitchUser? from);
                reader.TryGet("to", out TwitchUser? to);
                if ((from != null || to != null) &&
                    reader.TryGet("nb_viewer", out int? nbViewer) &&
                    reader.TryGet("is_incomming", out bool? isIncomming))
                    return new(new(from, to, (int)nbViewer!, (bool)isIncomming!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(RaidEventArgs obj, DataObject writer)
            {
                writer["from"] = obj.m_From;
                writer["to"] = obj.m_To;
                writer["nb_viewer"] = obj.m_NbViewers;
                writer["is_incomming"] = obj.m_IsIncomming;
            }
        }

        private readonly TwitchUser? m_From = from;
        private readonly TwitchUser? m_To = to;
        private readonly int m_NbViewers = nbViewers;
        private readonly bool m_IsIncomming = isIncomming;

        public TwitchUser? From => m_From;
        public TwitchUser? To => m_To;
        public int NbViewers => m_NbViewers;
        public bool IsIncomming => m_IsIncomming;
    }
}
