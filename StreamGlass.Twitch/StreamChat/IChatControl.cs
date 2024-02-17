namespace StreamGlass.Twitch.StreamChat
{
    public interface IChatControl
    {
        public void ToggleHighlightedUser(string userName);
        public string GetEmoteURL(string id);
    }
}
