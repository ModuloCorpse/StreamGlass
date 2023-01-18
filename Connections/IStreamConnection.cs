using StreamFeedstock.Controls;
using StreamGlass.Profile;

namespace StreamGlass.Connections
{
    public interface IStreamConnection : IConnection
    {
        public void SendMessage(string channel, string message);
        public string GetEmoteURL(string emoteID, BrushPaletteManager palette);
        public CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info);
    }
}
