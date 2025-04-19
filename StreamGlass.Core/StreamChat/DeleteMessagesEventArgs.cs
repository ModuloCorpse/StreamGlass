using CorpseLib.DataNotation;
using CorpseLib;

namespace StreamGlass.Core.StreamChat
{
    public class DeleteMessagesEventArgs(string[] ids)
    {
        public class DataSerializer : ADataSerializer<DeleteMessagesEventArgs>
        {
            protected override OperationResult<DeleteMessagesEventArgs> Deserialize(DataObject reader)
            {
                return new(new([.. reader.GetList<string>("ids")]));
            }

            protected override void Serialize(DeleteMessagesEventArgs obj, DataObject writer)
            {
                writer["ids"] = obj.m_IDs;
            }
        }

        private readonly string[] m_IDs = ids;
        public string[] IDs => m_IDs;
    }
}
