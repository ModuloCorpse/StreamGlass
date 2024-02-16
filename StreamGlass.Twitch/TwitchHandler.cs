using CorpseLib.Ini;
using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Core.Events;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class TwitchHandler(IniSection settings, TwitchAPI api) : ITwitchHandler
    {
        private readonly IniSection m_Settings = settings;
        private readonly TwitchAPI m_API = api;
        private string m_IRCChannel = string.Empty;

        internal void UpdateViewerCountOf(TwitchUser user)
        {
            List<TwitchStreamInfo> streamInfos = m_API.GetStreamInfoByID(user);
            if (streamInfos.Count == 1)
                StreamGlassContext.UpdateStatistic("viewer_count", streamInfos[0].ViewerCount);
        }

        public void SetIRCChannel(string channel) => m_IRCChannel = channel;

        public void OnBits(TwitchUser user, int bits, Text message)
        {
            StreamGlassContext.UpdateStatistic("last_bits_donor", user.DisplayName);
            StreamGlassContext.UpdateStatistic("last_bits_donation", bits);
            if (bits >= StreamGlassContext.GetStatistic("top_bits_donation", 0))
            {
                StreamGlassContext.UpdateStatistic("top_bits_donor", user.DisplayName);
                StreamGlassContext.UpdateStatistic("top_bits_donation", bits);
            }
            StreamGlassCanals.Emit("donation", new DonationEventArgs(user, bits, "Bits", message));
        }

        public void OnChatJoined() => StreamGlassCanals.Emit("chat_joined", m_IRCChannel);

        public void OnChatMessageRemoved(string messageID) => StreamGlassCanals.Emit("chat_clear_message", messageID);

        public void OnChatUserRemoved(string userID) => StreamGlassCanals.Emit("chat_clear_user", userID);

        public void OnChatClear() => StreamGlassCanals.Trigger("chat_clear");

        public void OnChatMessage(TwitchUser user, bool isHighlight, string messageId, string announcementColor, string messageColor, Text message)
        {
            StreamGlassCanals.Emit("chat_message", new UserMessage(user, isHighlight, messageId, announcementColor, messageColor, m_IRCChannel, message));
        }

        public void OnFollow(TwitchUser user)
        {
            StreamGlassContext.UpdateStatistic("last_follow", user.DisplayName);
            StreamGlassCanals.Emit("follow", new FollowEventArgs(user, new(string.Empty), 0, -1, -1));
        }

        public void OnRaided(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassContext.UpdateStatistic("last_raider", user.DisplayName);
            StreamGlassCanals.Emit("raid", new RaidEventArgs(user, self, nbViewer, true));
        }

        public void OnRaiding(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassCanals.Emit("raid", new RaidEventArgs(self, user, nbViewer, false));
        }

        public void OnReward(TwitchUser user, string reward, string input) => StreamGlassCanals.Emit("reward", new RewardEventArgs(user, reward, input));

        public void OnStreamStart() => StreamGlassCanals.Trigger("stream_start");

        public void OnStreamStop() => StreamGlassCanals.Trigger("stream_stop");

        public void OnGiftSub(TwitchUser? user, int tier, int nbGift)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            if (user != null)
            {
                StreamGlassContext.UpdateStatistic("last_gifter", user.DisplayName);
                StreamGlassContext.UpdateStatistic("last_nb_gift", nbGift);
                if (nbGift >= StreamGlassContext.GetStatistic("top_nb_gift", 0))
                {
                    StreamGlassContext.UpdateStatistic("top_gifter", user.DisplayName);
                    StreamGlassContext.UpdateStatistic("top_nb_gift", nbGift);
                }
            }
            StreamGlassCanals.Emit("gift_follow", new GiftFollowEventArgs(null, user, new(string.Empty), tier, -1, -1, nbGift!));
        }

        public void OnSharedGiftSub(TwitchUser? gifter, TwitchUser user, int tier, int monthGifted, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassCanals.Emit("gift_follow", new GiftFollowEventArgs(user, gifter, message, tier, monthGifted, monthStreak, 1));
        }

        public void OnSub(TwitchUser user, int tier, bool isGift)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            StreamGlassContext.UpdateStatistic("last_sub", user.DisplayName);
            if (isGift)
                StreamGlassCanals.Emit("gift_follow", new GiftFollowEventArgs(null, user, new(string.Empty), tier, -1, -1, -1));
            else
                StreamGlassCanals.Emit("follow", new FollowEventArgs(user, new(string.Empty), tier, -1, -1));
        }

        public void OnSharedSub(TwitchUser user, int tier, int monthTotal, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassContext.UpdateStatistic("last_sub", user.DisplayName);
            StreamGlassCanals.Emit("follow", new FollowEventArgs(user, message, tier, monthTotal, monthStreak));
        }

        public void OnUserJoinChat(TwitchUser user) => StreamGlassCanals.Emit("user_joined", user);

        public void UnhandledEventSub(string message) => TwitchEventSub.LOGGER.Log(message);

        public void OnMessageHeld(TwitchUser user, string messageID, Text message) => StreamGlassCanals.Emit("held_message", new UserMessage(user, false, messageID, string.Empty, string.Empty, string.Empty, message));

        public void OnHeldMessageTreated(string messageID) => StreamGlassCanals.Emit("held_message_moderated", messageID);

        public void OnBeingShoutout(TwitchUser from) => StreamGlassCanals.Emit("being_shoutout", from);

        public void OnShoutout(TwitchUser moderator, TwitchUser to) => StreamGlassCanals.Emit("shoutout", new ShoutoutEventArgs(moderator, to));
    }
}
