namespace StreamGlass.StreamChat
{
    public interface IStreamChat
    {
        public void SendMessage(string channel, string message);
        public string GetEmoteURL(string emoteID, BrushPaletteManager palette);
    }
}
