using CorpseLib.Ini;
using StreamGlass.Controls;
using StreamGlass.Profile;

namespace StreamGlass.Connections
{
    public abstract class AStreamConnection : AConnection
    {
        protected AStreamConnection(IniSection settings, StreamGlassWindow form): base(settings, form) { }

        public abstract void SendMessage(string channel, string message);
        public abstract CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info);
    }
}
