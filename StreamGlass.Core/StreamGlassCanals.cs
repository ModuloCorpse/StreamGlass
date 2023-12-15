using CorpseLib;
using StreamGlass.Core.Events;
using StreamGlass.Core.Profile;
using TwitchCorpse;

namespace StreamGlass.Core
{
    public static class StreamGlassCanals
    {
        public static readonly Canal<UserMessage> CHAT_MESSAGE = new();
        public static readonly Canal CHAT_CONNECTED = new();
        public static readonly Canal<string> CHAT_JOINED = new();
        public static readonly Canal<TwitchUser> USER_JOINED = new();
        public static readonly Canal<UpdateStreamInfoArgs> UPDATE_STREAM_INFO = new();
        public static readonly Canal STREAM_START = new();
        public static readonly Canal STREAM_STOP = new();
        public static readonly Canal<DonationEventArgs> DONATION = new();
        public static readonly Canal<FollowEventArgs> FOLLOW = new();
        public static readonly Canal<GiftFollowEventArgs> GIFT_FOLLOW = new();
        public static readonly Canal<RaidEventArgs> RAID = new();
        public static readonly Canal<RewardEventArgs> REWARD = new();
        public static readonly Canal<CommandEventArgs> COMMANDS = new();
        public static readonly Canal<string> PROFILE_CHANGED_MENU_ITEM = new();
        public static readonly Canal<BanEventArgs> BAN = new();
        public static readonly Canal<UserMessage> HELD_MESSAGE = new();
        public static readonly Canal<MessageAllowedEventArgs> ALLOW_MESSAGE = new();
        public static readonly Canal<string> HELD_MESSAGE_MODERATED = new();
        public static readonly Canal CHAT_CLEAR = new();
        public static readonly Canal<string> CHAT_CLEAR_USER = new();
        public static readonly Canal<string> CHAT_CLEAR_MESSAGE = new();
        public static readonly Canal<ShoutoutEventArgs> SHOUTOUT = new();
        public static readonly Canal<TwitchUser> BEING_SHOUTOUT = new();
        public static readonly Canal<uint> START_ADS = new();
    }
}