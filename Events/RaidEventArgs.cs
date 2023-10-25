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
                if (reader.TryGet("from_id", out string? fromID) &&
                    reader.TryGet("from", out string? from) &&
                    reader.TryGet("to_id", out string? toID) &&
                    reader.TryGet("to", out string? to) &&
                    reader.TryGet("nb_viewer", out int? nbViewer) &&
                    reader.TryGet("is_incomming", out bool? isIncomming))
                    return new(new(fromID!, from!, toID!, to!, (int)nbViewer!, (bool)isIncomming!));
                return new("Bad json", string.Empty);
            }

            protected override void Serialize(RaidEventArgs obj, JObject writer)
            {
                writer["from_id"] = obj.m_FromID;
                writer["from"] = obj.m_From;
                writer["to_id"] = obj.m_ToID;
                writer["to"] = obj.m_To;
                writer["nb_viewer"] = obj.m_NbViewers;
                writer["is_incomming"] = obj.m_IsIncomming;
            }
        }

        private readonly string m_FromID;
        private readonly string m_From;
        private readonly string m_ToID;
        private readonly string m_To;
        private readonly int m_NbViewers;
        private readonly bool m_IsIncomming;

        public string FromID => m_FromID;
        public string From => m_From;
        public string ToID => m_ToID;
        public string To => m_To;
        public int NbViewers => m_NbViewers;
        public bool IsIncomming => m_IsIncomming;

        public RaidEventArgs(string fromID, string from, string toID, string to, int nbViewers, bool isIncomming)
        {
            m_FromID = fromID;
            m_From = from;
            m_ToID = toID;
            m_To = to;
            m_NbViewers = nbViewers;
            m_IsIncomming = isIncomming;
        }
    }
}
