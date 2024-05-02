using CorpseLib.Actions;
using StreamGlass.Core;

namespace StreamGlass.Twitch.Actions
{
    public class TwitchAd(Core core) : AStreamGlassAction(ms_Definition)
    {
        private static readonly ActionDefinition ms_Definition = new ActionDefinition("TwitchAd", "Start a twitch ad")
            .AddArgument<uint>("duration", "Duration of the ad in seconds");
        private readonly Core m_Core = core;

        public override bool AllowDirectCall => true;
        public override bool AllowCLICall => true;
        public override bool AllowScriptCall => true;
        public override bool AllowRemoteCall => true;

        public override object?[] Call(object?[] args)
        {
            uint adDuration = Math.Clamp((uint)args[0]!, 1, 180);
            m_Core.StartAds(adDuration);
            return [];
        }
    }
}
