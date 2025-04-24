using CorpseLib.DataNotation;

namespace StreamGlass.Core.StreamChat
{
    public interface IMessageSourceHandler
    {
        void SendMessage(DataObject messageData);
        void DeleteMessage(string id);
    }
}
