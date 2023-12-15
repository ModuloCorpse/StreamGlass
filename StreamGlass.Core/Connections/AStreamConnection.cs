using CorpseLib.Ini;
using StreamGlass.Core.Controls;
using StreamGlass.Core.Profile;

namespace StreamGlass.Core.Connections
{
    public abstract class AStreamConnection : AConnection
    {
        protected AStreamConnection(IniSection settings): base(settings) { }

        public abstract void SendMessage(string message);
        public abstract CategoryInfo? SearchCategoryInfo(Window parent, CategoryInfo? info);
    }
}
