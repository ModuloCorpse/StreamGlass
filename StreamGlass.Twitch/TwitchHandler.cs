using CorpseLib.Ini;
using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Core.Events;
using System.Collections.Generic;
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
            StreamGlassCanals.DONATION.Emit(new(user, bits, "Bits", message));
        }

        public void OnChatJoined() => StreamGlassCanals.CHAT_JOINED.Emit(m_IRCChannel);

        public void OnChatMessageRemoved(string messageID) => StreamGlassCanals.CHAT_CLEAR_MESSAGE.Emit(messageID);

        public void OnChatUserRemoved(string userID) => StreamGlassCanals.CHAT_CLEAR_USER.Emit(userID);

        public void OnChatClear() => StreamGlassCanals.CHAT_CLEAR.Trigger();

        public void OnChatMessage(TwitchUser user, bool isHighlight, string messageId, string messageColor, Text message)
        {
            StreamGlassCanals.CHAT_MESSAGE.Emit(new(user, isHighlight, messageId, messageColor, m_IRCChannel, message));
        }

        public void OnFollow(TwitchUser user)
        {
            StreamGlassContext.UpdateStatistic("last_follow", user.DisplayName);
            StreamGlassCanals.FOLLOW.Emit(new(user, new(string.Empty), 0, -1, -1));
        }

        public void OnRaided(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassContext.UpdateStatistic("last_raider", user.DisplayName);
            StreamGlassCanals.RAID.Emit(new RaidEventArgs(user, self, nbViewer, true));
        }

        public void OnRaiding(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassCanals.RAID.Emit(new RaidEventArgs(self, user, nbViewer, false));
        }

        public void OnReward(TwitchUser user, string reward, string input) => StreamGlassCanals.REWARD.Emit(new(user, reward, input));

        public void OnStreamStart() => StreamGlassCanals.STREAM_START.Trigger();

        public void OnStreamStop() => StreamGlassCanals.STREAM_STOP.Trigger();

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
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(null, user, new(string.Empty), tier, -1, -1, nbGift!));
        }

        public void OnSharedGiftSub(TwitchUser user, TwitchUser recipient, int tier, int monthGifted, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(recipient, user, message, tier, monthGifted, monthStreak, 1));
        }

        public void OnSub(TwitchUser user, int tier, bool isGift)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            StreamGlassContext.UpdateStatistic("last_sub", user.DisplayName);
            if (isGift)
                StreamGlassCanals.GIFT_FOLLOW.Emit(new(null, user, new(string.Empty), tier, -1, -1, -1));
            else
                StreamGlassCanals.FOLLOW.Emit(new(user, new(string.Empty), tier, -1, -1));
        }

        public void OnSharedSub(TwitchUser user, int tier, int monthTotal, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassContext.UpdateStatistic("last_sub", user.DisplayName);
            StreamGlassCanals.FOLLOW.Emit(new(user, message, tier, monthTotal, monthStreak));
        }

        public void OnUserJoinChat(TwitchUser user) => StreamGlassCanals.USER_JOINED.Emit(user);

        public void UnhandledEventSub(string message) => TwitchEventSub.EVENTSUB.Log(message);

        public void OnMessageHeld(TwitchUser user, string messageID, Text message) => StreamGlassCanals.HELD_MESSAGE.Emit(new(user, false, messageID, string.Empty, string.Empty, message));

        public void OnHeldMessageTreated(string messageID) => StreamGlassCanals.HELD_MESSAGE_MODERATED.Emit(messageID);

        public void OnBeingShoutout(TwitchUser from) => StreamGlassCanals.BEING_SHOUTOUT.Emit(from);

        public void OnShoutout(TwitchUser moderator, TwitchUser to) => StreamGlassCanals.SHOUTOUT.Emit(new(moderator, to));
    }
}
