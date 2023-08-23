using CorpseLib.Ini;
using CorpseLib.StructuredText;
using StreamGlass.Events;
using TwitchCorpse;

namespace StreamGlass.Twitch
{
    public class TwitchHandler : ITwitchHandler
    {
        private readonly IniSection m_Settings;
        private readonly API m_API;
        private string m_IRCChannel = string.Empty;

        public TwitchHandler(IniSection settings, API api)
        {
            m_Settings = settings;
            m_API = api;
        }

        public void SetIRCChannel(string channel) => m_IRCChannel = channel;

        public void OnBits(User user, int bits, Text message) => StreamGlassCanals.DONATION.Emit(new(user.DisplayName, bits, "Bits", message));

        public void OnChatJoined() => StreamGlassCanals.CHAT_JOINED.Emit(m_IRCChannel);

        public void OnChatMessage(User user, bool isHighlight, string messageId, string messageColor, Text message)
        {
            StreamGlassCanals.CHAT_MESSAGE.Emit(new(user, isHighlight, messageId, messageColor, m_IRCChannel, message));
        }

        public void OnFollow(User user) => StreamGlassCanals.FOLLOW.Emit(new(user.DisplayName, new(""), 0, -1, -1));

        public void OnRaided(User user, int nbViewer)
        {
            User self = m_API.GetSelfUserInfo();
            StreamGlassCanals.RAID.Emit(new RaidEventArgs(user.ID, user.DisplayName, self.ID, self.DisplayName, nbViewer, true));
        }

        public void OnRaiding(User user, int nbViewer)
        {
            User self = m_API.GetSelfUserInfo();
            StreamGlassCanals.RAID.Emit(new RaidEventArgs(self.ID, self.DisplayName, user.ID, user.DisplayName, nbViewer, false));
        }

        public void OnReward(User user, string reward, string input) => StreamGlassCanals.REWARD.Emit(new(user.ID, user.DisplayName, reward, input));

        public void OnStreamStart() => StreamGlassCanals.STREAM_START.Trigger();

        public void OnStreamStop() => StreamGlassCanals.STREAM_STOP.Trigger();

        public void OnGiftSub(User? user, int tier, int nbGift)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(string.Empty, (user != null) ? user.DisplayName : "", new(""), tier, -1, -1, nbGift!));
        }

        public void OnSharedGiftSub(User user, User recipient, int tier, int monthGifted, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassCanals.GIFT_FOLLOW.Emit(new(recipient.DisplayName, user.DisplayName, message, tier, monthGifted, monthStreak, 1));
        }

        public void OnSub(User user, int tier, bool isGift)
        {
            if (m_Settings.Get("sub_mode") == "claimed")
                return;
            if (isGift)
                StreamGlassCanals.GIFT_FOLLOW.Emit(new(string.Empty, user.DisplayName, new(""), tier, -1, -1, -1));
            else
                StreamGlassCanals.FOLLOW.Emit(new(user.DisplayName, new(""), tier, -1, -1));
        }

        public void OnSharedSub(User user, int tier, int monthTotal, int monthStreak, Text message)
        {
            if (m_Settings.Get("sub_mode") == "all")
                return;
            StreamGlassCanals.FOLLOW.Emit(new(user.DisplayName, message, tier, monthTotal, monthStreak));
        }

        public void OnUserJoinChat(User user) => StreamGlassCanals.USER_JOINED.Emit(user);

        public void UnhandledEventSub(string message) => EventSub.EVENTSUB.Log(message);

        public void OnMessageHeld(User user, string messageID, Text message) => StreamGlassCanals.HELD_MESSAGE.Emit(new(user, false, messageID, string.Empty, string.Empty, message));

        public void OnHeldMessageTreated(string messageID) => StreamGlassCanals.HELD_MESSAGE_MODERATED.Emit(messageID);
    }
}
