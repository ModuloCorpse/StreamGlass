namespace StreamGlass.Twitch.IRC
{
    public interface IListener
    {
        public void OnConnected();
        public void OnJoinedChannel(string channel);
        public void OnMessageReceived(UserMessage message);
    }
}
