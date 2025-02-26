﻿using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Core.Profile;
using StreamGlass.Twitch.Events;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class Handler(Core core, Settings settings, TwitchAPI api) : ITwitchHandler
    {
        private readonly Core m_Core = core;
        private readonly List<IMessageFilter> m_GlobalFilters = [];
        private readonly List<IMessageFilter> m_MessageFilters = [ /*new FirstHalfBoldFilter()*/ ];
        private readonly List<IMessageFilter> m_OverlayFilters = [];
        private readonly Settings m_Settings = settings;
        private readonly TwitchAPI m_API = api;
        private string m_IRCChannel = string.Empty;

        internal bool UpdateViewerCountOf(TwitchUser user)
        {
            TwitchStreamInfo? streamInfo = m_API.GetStreamInfoByID(user);
            if (streamInfo != null)
            {
                StreamGlassContext.UpdateStringSource("viewer_count", streamInfo.ViewerCount.ToString());
                return true;
            }
            return false;
        }

        public void SetIRCChannel(string channel) => m_IRCChannel = channel;

        public void OnBits(TwitchUser user, int bits, Text message)
        {
            StreamGlassContext.UpdateStringSource("last_bits_donor", user.DisplayName);
            StreamGlassContext.UpdateStringSource("last_bits_donation", bits.ToString());
            string topBitsDonationStr = StreamGlassContext.GetStringSource("top_bits_donation", "0");
            if (int.TryParse(topBitsDonationStr, out int topBitsDonation))
            {
                if (bits >= topBitsDonation)
                {
                    StreamGlassContext.UpdateStringSource("top_bits_donor", user.DisplayName);
                    StreamGlassContext.UpdateStringSource("top_bits_donation", bits.ToString());
                }
            }
            StreamGlassCanals.Emit(TwitchPlugin.Canals.DONATION, new DonationEventArgs(user, bits, "Bits", message));
        }

        public void OnChatJoined() => StreamGlassCanals.Emit(TwitchPlugin.Canals.CHAT_JOINED, m_IRCChannel);

        public void OnChatMessageRemoved(string messageID) => StreamGlassCanals.Emit(TwitchPlugin.Canals.CHAT_CLEAR_MESSAGE, messageID);

        public void OnChatUserRemoved(string userID) => StreamGlassCanals.Emit(TwitchPlugin.Canals.CHAT_CLEAR_USER, userID);

        public void OnChatClear()
        {
            StreamGlassCanals.Trigger(TwitchPlugin.Canals.CHAT_CLEAR);
            StreamGlassCanals.Trigger(StreamGlassCanals.PROFILE_RESET);
        }

        private static uint GetUserType(TwitchUser user)
        {
            switch (user.UserType)
            {
                case TwitchUser.Type.NONE:
                {
                    return 0;
                    //return 1; for sub tier 1
                    //return 2; for sub tier 2
                    //return 3; for sub tier 3
                }
                case TwitchUser.Type.MOD:
                    return 4;
                case TwitchUser.Type.GLOBAL_MOD:
                    return 5;
                case TwitchUser.Type.ADMIN:
                    return 6;
                case TwitchUser.Type.STAFF:
                    return 7;
                case TwitchUser.Type.BROADCASTER:
                    return 8;
                case TwitchUser.Type.SELF:
                    return uint.MaxValue;
            }
            return 0;
        }

        public void OnChatMessage(TwitchChatMessage chatMessage)
        {
            TwitchUser user = chatMessage.User;
            Text message = chatMessage.Message;
            if (m_Core.IsIRCSelf(user))
                user = new(user.ID, user.Name, user.DisplayName, user.ProfileImageURL, TwitchUser.Type.SELF, [..user.Badges]);
            StreamGlassCanals.Emit(StreamGlassCanals.CHAT_MESSAGE, new UserMessage(GetUserType(user), message.ToString()));
            foreach (IMessageFilter globalFilter in m_GlobalFilters)
                message = globalFilter.Filter(message);
            Text messageToDisplay = (Text)message.Clone();
            foreach (IMessageFilter messageFilter in m_MessageFilters)
                messageToDisplay = messageFilter.Filter(messageToDisplay);
            StreamGlassCanals.Emit(TwitchPlugin.Canals.CHAT_MESSAGE, new Message(user, chatMessage.IsHighlight, chatMessage.MessageID, chatMessage.ReplyID, chatMessage.AnnouncementColor, chatMessage.MessageColor, m_IRCChannel, messageToDisplay));
            Text overlayMessage = (Text)message.Clone();
            foreach (IMessageFilter overlayFilter in m_OverlayFilters)
                overlayMessage = overlayFilter.Filter(overlayMessage);
            StreamGlassCanals.Emit(TwitchPlugin.Canals.OVERLAY_CHAT_MESSAGE, new Message(user, chatMessage.IsHighlight, chatMessage.MessageID, chatMessage.ReplyID, chatMessage.AnnouncementColor, chatMessage.MessageColor, m_IRCChannel, overlayMessage));
        }

        public void OnFollow(TwitchUser user)
        {
            StreamGlassContext.UpdateStringSource("last_follow", user.DisplayName);
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(user, new(string.Empty), 0, -1, -1));
        }

        public void OnRaided(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassContext.UpdateStringSource("last_raider", user.DisplayName);
            StreamGlassCanals.Emit(TwitchPlugin.Canals.RAID, new RaidEventArgs(user, self, nbViewer, true));
        }

        public void OnRaiding(TwitchUser user, int nbViewer)
        {
            TwitchUser self = m_API.GetSelfUserInfo();
            StreamGlassCanals.Emit(TwitchPlugin.Canals.RAID, new RaidEventArgs(self, user, nbViewer, false));
        }

        public void OnReward(TwitchUser user, string reward, string input) => StreamGlassCanals.Emit(TwitchPlugin.Canals.REWARD, new RewardEventArgs(user, reward, input));

        public void OnStreamStart()
        {
            StreamGlassCanals.Trigger(TwitchPlugin.Canals.STREAM_START);
            StreamGlassCanals.Trigger(StreamGlassCanals.PROFILE_RESET);
        }

        public void OnStreamStop() => StreamGlassCanals.Trigger(TwitchPlugin.Canals.STREAM_STOP);

        public void OnGiftSub(TwitchUser? user, int tier, int nbGift)
        {
            if (m_Settings.SubMode == "claimed")
                return;
            if (user != null)
            {
                StreamGlassContext.UpdateStringSource("last_gifter", user.DisplayName);
                StreamGlassContext.UpdateStringSource("last_nb_gift", nbGift.ToString());
                string topNbGiftStr = StreamGlassContext.GetStringSource("top_nb_gift", "0");
                if (int.TryParse(topNbGiftStr, out int topNbGift))
                {
                    if (nbGift >= topNbGift)
                    {
                        StreamGlassContext.UpdateStringSource("top_gifter", user.DisplayName);
                        StreamGlassContext.UpdateStringSource("top_nb_gift", nbGift.ToString());
                    }
                }
            }
            StreamGlassCanals.Emit(TwitchPlugin.Canals.GIFT_FOLLOW, new GiftFollowEventArgs(null, user, new(string.Empty), tier, -1, -1, nbGift!));
        }

        public void OnSharedGiftSub(TwitchUser? gifter, TwitchUser user, int tier, int monthGifted, int monthStreak, Text message)
        {
            if (m_Settings.SubMode == "all")
                return;
            StreamGlassCanals.Emit(TwitchPlugin.Canals.GIFT_FOLLOW, new GiftFollowEventArgs(user, gifter, message, tier, monthGifted, monthStreak, 1));
        }

        public void OnSub(TwitchUser user, int tier, bool isGift)
        {
            if (m_Settings.SubMode == "claimed")
                return;
            StreamGlassContext.UpdateStringSource("last_sub", user.DisplayName);
            if (isGift)
                StreamGlassCanals.Emit(TwitchPlugin.Canals.GIFT_FOLLOW, new GiftFollowEventArgs(null, user, new(string.Empty), tier, -1, -1, -1));
            else
                StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(user, new(string.Empty), tier, -1, -1));
        }

        public void OnSharedSub(TwitchUser user, int tier, int monthTotal, int monthStreak, Text message)
        {
            if (m_Settings.SubMode == "all")
                return;
            StreamGlassContext.UpdateStringSource("last_sub", user.DisplayName);
            StreamGlassCanals.Emit(TwitchPlugin.Canals.FOLLOW, new FollowEventArgs(user, message, tier, monthTotal, monthStreak));
        }

        public void OnUserJoinChat(TwitchUser user) => StreamGlassCanals.Emit(TwitchPlugin.Canals.USER_JOINED, user);

        public void UnhandledEventSub(string message) => TwitchEventSub.LOGGER.Log(message);

        public void OnMessageHeld(TwitchUser user, string messageID, Text message) => StreamGlassCanals.Emit(TwitchPlugin.Canals.HELD_MESSAGE, new Message(user, false, messageID, string.Empty, string.Empty, string.Empty, string.Empty, message));

        public void OnHeldMessageTreated(string messageID) => StreamGlassCanals.Emit(TwitchPlugin.Canals.HELD_MESSAGE_MODERATED, messageID);

        public void OnBeingShoutout(TwitchUser from) => StreamGlassCanals.Emit(TwitchPlugin.Canals.BEING_SHOUTOUT, from);

        public void OnShoutout(TwitchUser moderator, TwitchUser to) => StreamGlassCanals.Emit(TwitchPlugin.Canals.SHOUTOUT, new ShoutoutEventArgs(moderator, to));

        public void OnSharedChatStart() => StreamGlassCanals.Trigger(StreamGlassCanals.PROFILE_LOCK_ALL);
        public void OnSharedChatStop() => StreamGlassCanals.Trigger(StreamGlassCanals.PROFILE_UNLOCK_ALL);
    }
}
