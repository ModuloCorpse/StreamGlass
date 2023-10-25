using CorpseLib;
using CorpseLib.Web.API.Event;
using StreamGlass.Events;
using StreamGlass.Profile;
using StreamGlass.StreamChat;
using TwitchCorpse;

namespace StreamGlass
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

        public static EventManager CreateAPIEventManager()
        {
            EventManager eventManager = new();
            eventManager.RegisterCanal("chat_message", CHAT_MESSAGE);
            eventManager.RegisterCanal("chat_connected", CHAT_CONNECTED);
            eventManager.RegisterCanal("chat_joined", CHAT_JOINED);
            eventManager.RegisterCanal("user_joined", USER_JOINED);
            eventManager.RegisterCanal("update_stream_info", UPDATE_STREAM_INFO);
            eventManager.RegisterCanal("stream_start", STREAM_START);
            eventManager.RegisterCanal("stream_stop", STREAM_STOP);
            eventManager.RegisterCanal("donation", DONATION);
            eventManager.RegisterCanal("follow", FOLLOW);
            eventManager.RegisterCanal("gift_follow", GIFT_FOLLOW);
            eventManager.RegisterCanal("raid", RAID);
            eventManager.RegisterCanal("reward", REWARD);
            eventManager.RegisterCanal("commands", COMMANDS);
            eventManager.RegisterCanal("profile_changed_menu_item", PROFILE_CHANGED_MENU_ITEM);
            eventManager.RegisterCanal("ban", BAN);
            eventManager.RegisterCanal("held_message", HELD_MESSAGE);
            eventManager.RegisterCanal("allow_message", ALLOW_MESSAGE);
            eventManager.RegisterCanal("held_message_moderated", HELD_MESSAGE_MODERATED);
            eventManager.RegisterCanal("chat_clear", CHAT_CLEAR);
            eventManager.RegisterCanal("chat_clear_user", CHAT_CLEAR_USER);
            eventManager.RegisterCanal("chat_clear_message", CHAT_CLEAR_MESSAGE);
            return eventManager;
        }
    }
}