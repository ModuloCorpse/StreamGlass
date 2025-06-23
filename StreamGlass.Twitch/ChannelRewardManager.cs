using CorpseLib.StructuredText;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class ChannelRewardManager
    {
        private readonly Dictionary<string, ChannelReward> m_Rewards = new();

        public void DeleteChannelReward(string rewardID) => m_Rewards.Remove(rewardID);

        public void AddChannelReward(TwitchRewardInfo rewardInfo)
        {
            string id = rewardInfo.ID;
            if (m_Rewards.TryGetValue(id, out ChannelReward? value))
            {
                TwitchPlugin.TWITCH_PLUGIN_LOGGER.Log($"Updating channel reward {rewardInfo.Title}");
                value.UpdateRewardInfo(rewardInfo);
            }
            else
            {
                TwitchPlugin.TWITCH_PLUGIN_LOGGER.Log($"Adding channel reward {rewardInfo.Title}");
                m_Rewards[id] = new ChannelReward(rewardInfo);
            }
        }

        public void AddChannelRewards(List<TwitchRewardInfo> rewardInfos)
        {
            foreach (TwitchRewardInfo rewardInfo in rewardInfos)
                AddChannelReward(rewardInfo);
        }

        //TODO handle fulfilled reward

        public void ClaimReward(TwitchUser user, TwitchRewardRedemptionInfo redemption, Text input)
        {
            if (m_Rewards.TryGetValue(redemption.RewardID, out ChannelReward? reward))
                reward.Claim(user, redemption, input);
        }
    }
}
