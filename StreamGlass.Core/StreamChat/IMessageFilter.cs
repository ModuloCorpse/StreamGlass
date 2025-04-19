using CorpseLib.StructuredText;

namespace StreamGlass.Core.StreamChat
{
    public interface IMessageFilter
    {
        Text Filter(Text message);
    }
}
