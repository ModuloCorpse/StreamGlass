using CorpseLib;
using StreamGlass.Events;
using StreamGlass.Profile;
using StreamGlass.StreamChat;

namespace StreamGlass
{
    public static class StreamGlassCanals
    {
        public static Canal<UserMessage> CHAT_MESSAGE = new();
        public static Canal CHAT_CONNECTED = new();
        public static Canal<string> CHAT_JOINED = new();
        public static Canal<User> USER_JOINED = new();
        public static Canal<UpdateStreamInfoArgs> UPDATE_STREAM_INFO = new();
        public static Canal STREAM_START = new();
        public static Canal STREAM_STOP = new();
        public static Canal<DonationEventArgs> DONATION = new();
        public static Canal<FollowEventArgs> FOLLOW = new();
        public static Canal<GiftFollowEventArgs> GIFT_FOLLOW = new();
        public static Canal<RaidEventArgs> RAID = new();
        public static Canal<RewardEventArgs> REWARD = new();
        public static Canal<CommandEventArgs> COMMANDS = new();
        public static Canal<string> PROFILE_CHANGED_MENU_ITEM = new();
        public static Canal<BanEventArgs> BAN = new();
        public static Canal<UserMessage> HELD_MESSAGE = new();
        public static Canal<MessageAllowedEventArgs> ALLOW_MESSAGE = new();
        public static Canal<string> HELD_MESSAGE_MODERATED = new();
        public static Canal CHAT_CLEAR = new();
        public static Canal<string> CHAT_CLEAR_USER = new();
        public static Canal<string> CHAT_CLEAR_MESSAGE = new();
    }
}