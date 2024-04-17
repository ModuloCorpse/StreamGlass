using CorpseLib.StructuredText;

namespace StreamGlass.Twitch
{
    public interface IMessageFilter
    {
        Text Filter(Text message);
    }
}
