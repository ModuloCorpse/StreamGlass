using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Twitch.Events;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class ChannelReward(TwitchRewardInfo info)
    {
        private TwitchRewardInfo m_Info = info;

        public void UpdateRewardInfo(TwitchRewardInfo info) => m_Info = info;

        public void Claim(TwitchUser user, TwitchRewardRedemptionInfo redemption, Text input)
        {
            TwitchPlugin.TWITCH_PLUGIN_LOGGER.Log($"Claiming channel reward {m_Info.Title}");
            StreamGlassCanals.Emit(TwitchPlugin.Canals.REWARD, new RewardEventArgs(user, m_Info.Title, input));
        }
    }
}
