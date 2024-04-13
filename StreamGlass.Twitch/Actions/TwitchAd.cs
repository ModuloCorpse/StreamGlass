using CorpseLib.Actions;

namespace StreamGlass.Twitch.Actions
{
    public class TwitchAd(Core core) : AAction(ms_Definition)
    {
        private static readonly ActionDefinition ms_Definition = new ActionDefinition("TwitchAd")
            .AddArgument<uint>("duration");
        private readonly Core m_Core = core;

        public override object?[] Call(object?[] args)
        {
            uint adDuration = Math.Clamp((uint)args[0]!, 1, 180);
            m_Core.StartAds(adDuration);
            return [];
        }
    }
}
