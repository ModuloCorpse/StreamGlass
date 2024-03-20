using CorpseLib;
using CorpseLib.Shell;

namespace StreamGlass.Twitch.Commands
{
    public class TwitchAd(TwitchCore core) : Command("TwitchAd")
    {
        private readonly TwitchCore m_Core = core;

        protected override OperationResult<string> Execute(string[] args)
        {
            if (args.Length == 0)
                return new("Bad arguments", "Not enough argument");
            if (args.Length > 1)
                return new("Bad arguments", "Too much arguments");
            if (uint.TryParse(args[0], out uint adDuration))
                return new("Invalid argument", "Duration is not a valid number");
            if (adDuration == 0 || adDuration > 180)
                return new("Invalid argument", "Duration should be between 0 and 180 seconds");
            m_Core.StartAds(adDuration);
            return new("Ad started");

        }
    }
}
