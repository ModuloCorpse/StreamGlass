using Newtonsoft.Json.Linq;
using Quicksand.Web.Http;
using StreamGlass.Twitch.IRC;

namespace StreamGlass.Twitch
{
    public static class API
    {
        private static Client? ms_IRCClient = null;
        private static Authenticator? ms_Authenticator = null;
        private static string ms_ClientID = "";
        private static string ms_AccessToken = "";

        private delegate ManagedRequest CreateRequest();

        public static void Init(Client iRCClient, Authenticator authenticator)
        {
            ms_IRCClient = iRCClient;
            ms_Authenticator = authenticator;
        }

        internal static void Authenticate(string clientID, string accessToken)
        {
            ms_ClientID = clientID;
            ms_AccessToken = accessToken;
        }

        internal static void RefreshAuth()
        {
            if (ms_Authenticator != null)
            {
                ms_AccessToken = ms_Authenticator.RefreshToken();
                ms_IRCClient?.SendAuth("StreamGlass", ms_AccessToken);
            }
        }

        public static void SendIRCMessage(string channel, string message) => ms_IRCClient?.SendMessage(channel, message);

        private static Response? APICall(CreateRequest createRequest)
        {
            Response? response;
            do
            {
                response = createRequest().AddHeaderField("Client-Id", ms_ClientID)
                    .AddHeaderField("Authorization", string.Format("Bearer {0}", ms_AccessToken))
                    .Send();
                if (response != null && response.StatusCode == 401)
                {
                    if (ms_Authenticator != null)
                        RefreshAuth();
                    else
                        return null;
                }
            } while (response != null && response.StatusCode == 401);
            return response;
        }

        private static UserInfo? GetUserInfo(string content)
        {
            JObject responseJson = JObject.Parse(content);
            JArray? datas = (JArray?)responseJson["data"];
            if (datas != null && datas.Count > 0)
            {
                JObject data = (JObject)datas[0];
                string? id = (string?)data["id"];
                string? login = (string?)data["login"];
                string? displayName = (string?)data["display_name"];
                string? type = (string?)data["type"];
                string? broadcasterType = (string?)data["broadcaster_type"];
                string? description = (string?)data["description"];
                if (id != null && login != null && displayName != null && type != null && broadcasterType != null && description != null)
                    return new UserInfo(id, login, displayName, type, broadcasterType, description);
            }
            return null;
        }

        public static string GetSelfUserID()
        {
            Response? response = APICall(() => new GetRequest(string.Format("https://api.twitch.tv/helix/users")));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    return ret.ID;
                return "";
            }
            return "";
        }

        public static UserInfo? GetUserInfoFromID(string id, APICache cache)
        {
            UserInfo? userInfo = cache.GetUserInfoByID(id);
            if (userInfo != null)
                return userInfo;
            Response? response = APICall(() => new GetRequest(string.Format("https://api.twitch.tv/helix/users?id={0}", id)));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    cache.AddUserInfo(ret);
                return ret;
            }
            return null;
        }

        public static UserInfo? GetUserInfoFromLogin(string login, APICache cache)
        {
            UserInfo? userInfo = cache.GetUserInfoByLogin(login);
            if (userInfo != null)
                return userInfo;
            Response? response = APICall(() => new GetRequest(string.Format("https://api.twitch.tv/helix/users?login={0}", login)));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    cache.AddUserInfo(ret);
                return ret;
            }
            return null;
        }

        private static StreamInfo? GetStreamInfo(string content, APICache cache)
        {
            JObject responseJson = JObject.Parse(content);
            JArray? datas = (JArray?)responseJson["data"];
            if (datas != null && datas.Count > 0)
            {
                JObject data = (JObject)datas[0];
                string? id = (string?)data["id"];
                string? userID = (string?)data["user_id"];
                UserInfo? broadcasterInfo = null;
                if (userID != null)
                    broadcasterInfo = GetUserInfoFromID(userID, cache);
                string? gameID = (string?)data["game_id"];
                string? gameName = (string?)data["game_name"];
                string? type = (string?)data["type"];
                string? title = (string?)data["title"];
                object? ret = (int?)data["viewer_count"];
                int viewers = (ret != null) ? (int)ret : -1;
                string? language = (string?)data["language"];
                ret = (bool?)data["is_mature"];
                bool isMature = (ret != null) && (bool)ret;
                if (id != null && broadcasterInfo != null && gameID != null && gameName != null && type != null && title != null && viewers != -1 && language != null)
                    return new StreamInfo(broadcasterInfo, id, gameID, gameName, type, title, language, viewers, isMature);
            }
            return null;
        }

        public static StreamInfo? GetStreamInfoFromLogin(string login, APICache cache)
        {
            StreamInfo? streamInfo = cache.GetStreamInfoByLogin(login);
            if (streamInfo != null)
                return streamInfo;
            Response? response = APICall(() => new GetRequest(string.Format("https://api.twitch.tv/helix/streams?user_login={0}", login)));
            if (response != null)
            {
                StreamInfo? ret = GetStreamInfo(response.Body, cache);
                if (ret != null)
                    cache.AddStreamInfo(ret);
                return ret;
            }
            return null;
        }

        private static ChannelInfo? GetChannelInfo(string content, UserInfo broadcasterInfo)
        {
            JObject responseJson = JObject.Parse(content);
            JArray? datas = (JArray?)responseJson["data"];
            if (datas != null && datas.Count > 0)
            {
                JObject data = (JObject)datas[0];
                string? gameID = (string?)data["game_id"];
                string? gameName = (string?)data["game_name"];
                string? title = (string?)data["title"];
                string? language = (string?)data["broadcaster_language"];
                if (gameID != null && gameName != null && title != null && language != null)
                    return new ChannelInfo(broadcasterInfo, gameID, gameName, title, language);
            }
            return null;
        }

        public static ChannelInfo? GetChannelInfoFromLogin(string login, APICache cache)
        {
            ChannelInfo? channelInfo = cache.GetChannelInfoByLogin(login);
            if (channelInfo != null)
                return channelInfo;
            UserInfo? broadcasterInfo = GetUserInfoFromLogin(login, cache);
            if (broadcasterInfo != null)
            {
                Response? response = APICall(() => new GetRequest(string.Format("https://api.twitch.tv/helix/channels?broadcaster_id={0}", broadcasterInfo.ID)));
                if (response != null)
                {
                    ChannelInfo? ret = GetChannelInfo(response.Body, broadcasterInfo);
                    if (ret != null)
                        cache.AddChannelInfo(ret);
                    return ret;
                }
            }
            return null;
        }
    }
}
