using StreamFeedstock.Controls;
using StreamGlass.Profile;
using StreamGlass.Settings;

namespace StreamGlass.Connections
{
    public abstract class AStreamConnection : AConnection
    {
        protected AStreamConnection(Data settings, StreamGlassWindow form, string settingsSection): base(settings, form, settingsSection) { }

        public abstract void SendMessage(string channel, string message);
        public abstract string GetEmoteURL(string emoteID, BrushPaletteManager palette);
        public abstract CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info);
    }
}
