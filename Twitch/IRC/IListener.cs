namespace StreamGlass.Twitch.IRC
{
    public interface IListener
    {
        public void OnConnected(Message message);
        public void OnJoinedChannel(Message message);
        public void OnMessageReceived(Message message);
    }
}
