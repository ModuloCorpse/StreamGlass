using CorpseLib;
using StreamGlass.API.Overlay;
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
        public static readonly Canal<ShoutoutEventArgs> SHOUTOUT = new();
        public static readonly Canal<TwitchUser> BEING_SHOUTOUT = new();
        public static readonly Canal<uint> START_ADS = new();

        public static OverlayWebsocketEndpoint CreateAPIEventEndpoint()
        {
            OverlayWebsocketEndpoint overlayWebsocketEndpoint = new();
            overlayWebsocketEndpoint.RegisterCanal("chat_message", CHAT_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("chat_connected", CHAT_CONNECTED);
            overlayWebsocketEndpoint.RegisterCanal("chat_joined", CHAT_JOINED);
            overlayWebsocketEndpoint.RegisterCanal("user_joined", USER_JOINED);
            overlayWebsocketEndpoint.RegisterCanal("update_stream_info", UPDATE_STREAM_INFO);
            overlayWebsocketEndpoint.RegisterCanal("stream_start", STREAM_START);
            overlayWebsocketEndpoint.RegisterCanal("stream_stop", STREAM_STOP);
            overlayWebsocketEndpoint.RegisterCanal("donation", DONATION);
            overlayWebsocketEndpoint.RegisterCanal("follow", FOLLOW);
            overlayWebsocketEndpoint.RegisterCanal("gift_follow", GIFT_FOLLOW);
            overlayWebsocketEndpoint.RegisterCanal("raid", RAID);
            overlayWebsocketEndpoint.RegisterCanal("reward", REWARD);
            overlayWebsocketEndpoint.RegisterCanal("commands", COMMANDS);
            overlayWebsocketEndpoint.RegisterCanal("profile_changed_menu_item", PROFILE_CHANGED_MENU_ITEM);
            overlayWebsocketEndpoint.RegisterCanal("ban", BAN);
            overlayWebsocketEndpoint.RegisterCanal("held_message", HELD_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("allow_message", ALLOW_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("held_message_moderated", HELD_MESSAGE_MODERATED);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear", CHAT_CLEAR);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear_user", CHAT_CLEAR_USER);
            overlayWebsocketEndpoint.RegisterCanal("chat_clear_message", CHAT_CLEAR_MESSAGE);
            overlayWebsocketEndpoint.RegisterCanal("shoutout", SHOUTOUT);
            overlayWebsocketEndpoint.RegisterCanal("being_shoutout", BEING_SHOUTOUT);
            overlayWebsocketEndpoint.RegisterCanal("start_ads", START_ADS);
            return overlayWebsocketEndpoint;
        }
    }
}