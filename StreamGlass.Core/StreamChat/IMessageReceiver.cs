namespace StreamGlass.Core.StreamChat
{
    public interface IMessageReceiver
    {
        public void AddMessage(Message message);
        public void RemoveMessages(string[] messageIDs);
    }
}
