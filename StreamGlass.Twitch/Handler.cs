using CorpseLib;
using CorpseLib.StructuredText;
using CorpseLib.Translation;
using StreamGlass.Core;
using StreamGlass.Core.StreamChat;
using StreamGlass.Twitch.Events;
using System.Windows.Media;
using TwitchCorpse;
using TwitchCorpse.API;

namespace StreamGlass.Twitch
{
    public class Handler(Core core, Settings settings, TwitchAPI api, MessageSource messageSource) : ITwitchHandler
    {
        //Key : Twitch, Value : StreamGlass
        private readonly BiMap<string, string> m_UserAssociationTable = [];
        //Key : Twitch, Value : StreamGlass
        private readonly BiMap<string, string> m_MessageAssociationTable = [];
        private readonly Dictionary<string, TwitchUser> m_Users = [];
        private readonly Core m_Core = core;
        private readonly Settings m_Settings = settings;
        private readonly TwitchAPI m_API = api;
        private readonly MessageSource m_MessageSource = messageSource;
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

        public void RemoveMessage(string id) => m_MessageAssociationTable.RemoveByKey(id);

        public void OnChatMessageRemoved(string messageID)
        {
            if (m_MessageAssociationTable.TryGetValue(messageID, out string? id))
                m_MessageSource.RemoveMessages([id!]);
        }

        public void OnChatUserRemoved(string userID)
        {
            if (m_UserAssociationTable.TryGetValue(userID, out string? id))
                m_MessageSource.RemoveAllMessagesFrom(id!);
        }

        public void OnChatClear()
        {
            m_MessageSource.ClearMessages();
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
            int timestamp = 0; //TODO
            string userID;
            uint userType = GetUserType(user);
            ChatUserInfo chatUserInfo = new(user.DisplayName, userType);
            if (!string.IsNullOrEmpty(chatMessage.MessageColor))
                chatUserInfo.SetColor((Color)ColorConverter.ConvertFromString(chatMessage.MessageColor));
            if (m_UserAssociationTable.ContainsKey(user.ID))
            {
                userID = m_UserAssociationTable.GetValue(user.ID);
                //Here to handle changes in twitch user name, color or role
                m_MessageSource.UpdateChatUser(userID, chatUserInfo);
            }
            else
            {
                userID = m_MessageSource.NewChatUser(chatUserInfo);
                m_UserAssociationTable.Add(user.ID, userID);
            }
            m_Users[user.ID] = user;
            MessageInfo messageInfo = new(userID, message, timestamp);
            foreach (TwitchBadgeInfo badgeInfo in user.Badges)
                messageInfo.AddBadge(badgeInfo.URL4x);
            if (m_MessageAssociationTable.TryGetValue(chatMessage.ReplyID, out string? replyID))
                messageInfo.SetReplyID(replyID!);
            if (!string.IsNullOrEmpty(chatMessage.AnnouncementColor))
                messageInfo.SetBorderColor((Color)ColorConverter.ConvertFromString(chatMessage.AnnouncementColor));
            messageInfo.SetIsHighlighted(chatMessage.IsHighlight || (user.UserType > TwitchUser.Type.MOD && user.UserType < TwitchUser.Type.BROADCASTER));
            string messageID = m_MessageSource.PostMessage(messageInfo);
            if (!string.IsNullOrEmpty(messageID))
                m_MessageAssociationTable.Add(chatMessage.MessageID, messageID);
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

        public void OnSharedChatStart() { }
        public void OnSharedChatStop() { }

        public TwitchUser? GetUserFromMessage(StreamGlass.Core.StreamChat.Message message)
        {
            if (m_UserAssociationTable.TryGetKey(message.User.ID, out string? userID))
            {
                if (m_Users.TryGetValue(userID!, out TwitchUser? user))
                {
                    return user;
                }
            }
            return null;
        }

        public string GetMessageID(string id)
        {
            if (m_MessageAssociationTable.TryGetKey(id, out string? messageID))
                return messageID!;
            return string.Empty;
        }

        internal void UnregisterChatContextMenu(TranslationKey key) => m_MessageSource.UnregisterChatContextMenu(key);
    }
}
