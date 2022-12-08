using Quicksand.Web.Http;
using StreamFeedstock;
using StreamFeedstock.Controls;
using System;
using System.Collections.Generic;

namespace StreamGlass.Twitch
{
    public static class API
    {
        private static string ms_EmoteURLTemplate = "";
        private static readonly HashSet<string> ms_LoadedChannelIDEmoteSets = new();
        private static readonly HashSet<string> ms_LoadedChannelEmoteSets = new();
        private static readonly HashSet<string> ms_LoadedEmoteSets = new();
        private static readonly APICache ms_Cache = new();
        private static OAuthToken? ms_AccessToken = null;

        private delegate ManagedRequest CreateRequest();

        internal static void Authenticate(OAuthToken accessToken)
        {
            ms_AccessToken = accessToken;
        }

        internal static void RefreshAuth()
        {
            if (ms_AccessToken != null)
                ms_AccessToken.Refresh();
        }

        private static Response? APICall(OAuthToken? token, ManagedRequest request)
        {
            if (token == null)
                return null;
            request = request.AddHeaderField("Client-Id", token.ClientID).AddHeaderField("Authorization", string.Format("Bearer {0}", token.Token));
            Response? response = request.Send();
            if (response != null && response.StatusCode == 401)
            {
                token.Refresh();
                request.Reset();
                response = request.Send();
            }
            return response;
        }

        private static void LoadEmoteContent(Json data)
        {
            List<string> format = data.GetList<string>("format");
            List<string> scale = data.GetList<string>("scale");
            List<string> themeMode = data.GetList<string>("theme_mode");
            if (data.TryGet("id", out string? id) &&
                data.TryGet("name", out string? name) &&
                data.TryGet("emote_type", out string? emoteType) &&
                format.Count != 0 && scale.Count != 0 && themeMode.Count != 0)
                ms_Cache.AddEmote(new(id!, name!, emoteType!, format, scale, themeMode));
        }

        private static Json JsonParse(string content)
        {
            if (string.IsNullOrEmpty(content))
                return new();
            try
            {
                return new Json(content);
            } catch (Exception e)
            {
                Logger.Log("API", string.Format("Json Exception: {0}", e));
                return new();
            }
        }

        private static Json LoadEmoteSetContent(string content)
        {
            Json responseJson = JsonParse(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            foreach (Json data in datas)
                LoadEmoteContent(data);
            return responseJson;
        }

        public static void LoadGlobalEmoteSet()
        {
            Response? response = APICall(ms_AccessToken, new GetRequest("https://api.twitch.tv/helix/chat/emotes/global"));
            if (response != null)
            {
                Json responseJson = LoadEmoteSetContent(response.Body);
                if (responseJson.TryGet("template", out string? template))
                    ms_EmoteURLTemplate = template!;
            }
        }

        public static void ReloadChannelEmoteSetFromID(string id)
        {
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/chat/emotes?broadcaster_id={0}", id)));
            if (response != null)
                LoadEmoteSetContent(response.Body);
        }

        public static void LoadChannelEmoteSetFromID(string id)
        {
            if (ms_LoadedChannelIDEmoteSets.Contains(id))
                return;
            ms_LoadedChannelIDEmoteSets.Add(id);
            ReloadChannelEmoteSetFromID(id);
        }

        public static void ReloadChannelEmoteSetFromLogin(string login)
        {
            UserInfo? userInfo = GetUserInfoFromLogin(login);
            if (userInfo != null)
                LoadChannelEmoteSetFromID(userInfo.ID);
        }

        public static void LoadChannelEmoteSetFromLogin(string login)
        {
            if (ms_LoadedChannelEmoteSets.Contains(login))
                return;
            ms_LoadedChannelEmoteSets.Add(login);
            ReloadChannelEmoteSetFromLogin(login);
        }

        public static void ReloadEmoteSet(string emoteSetID)
        {
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/chat/emotes/set?emote_set_id={0}", emoteSetID)));
            if (response != null)
                LoadEmoteSetContent(response.Body);
        }

        public static void LoadEmoteSet(string emoteSetID)
        {
            if (ms_LoadedEmoteSets.Contains(emoteSetID))
                return;
            ms_LoadedEmoteSets.Add(emoteSetID);
            ReloadEmoteSet(emoteSetID);
        }

        public static void LoadEmoteSetFromFollowedChannelOfLogin(string login)
        {
            List<string> followedBy = API.GetChannelFollowedByLogin(login);
            foreach (string followedID in followedBy)
                API.LoadChannelEmoteSetFromID(followedID);
        }

        public static void LoadEmoteSetFromFollowedChannelOfID(string id)
        {
            List<string> followedBy = GetChannelFollowedByID(id);
            foreach (string followedID in followedBy)
                LoadChannelEmoteSetFromID(followedID);
        }

        public static string GetEmoteURL(string id, BrushPalette.Type paletteType)
        {
            EmoteInfo? emoteInfo = ms_Cache.GetEmote(id);
            if (emoteInfo != null)
            {
                //TODO
                string format = "static";
                string scale = "1.0";
                string theme_mode = "dark";
                return ms_EmoteURLTemplate.Replace("{{id}}", id).Replace("{{format}}", format).Replace("{{scale}}", scale).Replace("{{theme_mode}}", theme_mode);
            }
            return "";
        }

        public static EmoteInfo? GetEmoteFromID(string id)
        {
            EmoteInfo? emoteInfo = ms_Cache.GetEmote(id);
            if (emoteInfo != null)
                return emoteInfo;
            return null;
        }

        private static UserInfo? GetUserInfo(string content)
        {
            Json responseJson = JsonParse(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            if (datas.Count > 0)
            {
                Json data = datas[0];
                if (data.TryGet("id", out string? id) &&
                    data.TryGet("login", out string? login) &&
                    data.TryGet("display_name", out string? displayName) &&
                    data.TryGet("type", out string? type) &&
                    data.TryGet("broadcaster_type", out string? broadcasterType) &&
                    data.TryGet("description", out string? description))
                    return new UserInfo(id!, login!, displayName!, type!, broadcasterType!, description!);
            }
            return null;
        }

        public static UserInfo? GetUserInfoOfToken(OAuthToken token)
        {
            Response? response = APICall(token, new GetRequest("https://api.twitch.tv/helix/users"));
            if (response != null)
                return GetUserInfo(response.Body);
            return null;
        }

        public static string GetSelfUserID()
        {
            Response? response = APICall(ms_AccessToken, new GetRequest("https://api.twitch.tv/helix/users"));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    return ret.ID;
                return "";
            }
            return "";
        }

        public static UserInfo? GetUserInfoFromID(string id)
        {
            UserInfo? userInfo = null;
            userInfo ??= ms_Cache.GetUserInfoByID(id);
            if (userInfo != null)
                return userInfo;
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/users?id={0}", id)));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    ms_Cache.AddUserInfo(ret);
                return ret;
            }
            return null;
        }

        public static UserInfo? GetUserInfoFromLogin(string login)
        {
            UserInfo? userInfo = null;
            userInfo ??= ms_Cache.GetUserInfoByLogin(login);
            if (userInfo != null)
                return userInfo;
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/users?login={0}", login)));
            if (response != null)
            {
                UserInfo? ret = GetUserInfo(response.Body);
                if (ret != null)
                    ms_Cache.AddUserInfo(ret);
                return ret;
            }
            return null;
        }

        private static StreamInfo? GetStreamInfo(string content)
        {
            Json responseJson = JsonParse(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            if (datas.Count > 0)
            {
                Json data = datas[0];
                UserInfo? broadcasterInfo = null;
                if (data.TryGet("user_id", out string? userID))
                    broadcasterInfo = GetUserInfoFromID(userID!);
                int viewers = data.GetOrDefault("viewer_count", -1);
                if (data.TryGet("id", out string? id) &&
                    broadcasterInfo != null &&
                    data.TryGet("game_id", out string? gameID) &&
                    data.TryGet("game_name", out string? gameName) &&
                    data.TryGet("type", out string? type) &&
                    data.TryGet("title", out string? title) &&
                    viewers != -1 &&
                    data.TryGet("language", out string? language))
                    return new StreamInfo(broadcasterInfo, id!, gameID!, gameName!, type!, title!, language!, viewers, data.GetOrDefault("is_mature", false));
            }
            return null;
        }

        public static StreamInfo? GetStreamInfoFromLogin(string login)
        {
            StreamInfo? streamInfo = null;
            if (streamInfo != null)
                return streamInfo;
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/streams?user_login={0}", login)));
            if (response != null)
                return GetStreamInfo(response.Body);
            return null;
        }

        private static ChannelInfo? GetChannelInfo(string content, UserInfo broadcasterInfo)
        {
            Json responseJson = JsonParse(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            if (datas.Count > 0)
            {
                Json data = datas[0];
                if (data.TryGet("game_id", out string? gameID) &&
                    data.TryGet("game_name", out string? gameName) &&
                    data.TryGet("title", out string? title) &&
                    data.TryGet("broadcaster_language", out string? language))
                    return new ChannelInfo(broadcasterInfo, gameID!, gameName!, title!, language!);
            }
            return null;
        }

        public static ChannelInfo? GetChannelInfoFromLogin(string login)
        {
            ChannelInfo? channelInfo = null;
            if (channelInfo != null)
                return channelInfo;
            UserInfo? broadcasterInfo = GetUserInfoFromLogin(login);
            if (broadcasterInfo != null)
            {
                Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/channels?broadcaster_id={0}", broadcasterInfo.ID)));
                if (response != null)
                    return GetChannelInfo(response.Body, broadcasterInfo);
            }
            return null;
        }

        public static bool SetChannelInfoFromLogin(string login, string title, string gameID, string language = "")
        {
            UserInfo? broadcasterInfo = GetUserInfoFromLogin(login);
            if (broadcasterInfo != null)
                return SetChannelInfoFromID(broadcasterInfo.ID, title, gameID, language);
            return false;
        }

        public static bool SetChannelInfoFromID(string id, string title, string gameID, string language = "")
        {
            if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(gameID) && string.IsNullOrEmpty(language))
                return false;
            Json body = new();
            if (!string.IsNullOrEmpty(title))
                body.Set("title", title);
            if (!string.IsNullOrEmpty(gameID))
                body.Set("game_id", gameID);
            if (!string.IsNullOrEmpty(language))
                body.Set("broadcaster_language", language);
            Response? response = APICall(ms_AccessToken, new PatchRequest(string.Format("https://api.twitch.tv/helix/channels?broadcaster_id={0}", id), body.ToString()).AddHeaderField("Content-Type", "application/json"));
            return (response != null && response.StatusCode == 204);
        }

        private static void LoadChannelFollowedByResponse(Response response, ref List<string> ret)
        {
            Json responseJson = JsonParse(response.Body);
            foreach (Json data in responseJson.GetList<Json>("data"))
            {
                if (data.TryGet("to_id", out string? toID))
                    ret.Add(toID!);
            }
        }

        public static List<string> GetChannelFollowedByID(string id)
        {
            List<string> ret = new();
            Response? response = APICall(ms_AccessToken, new GetRequest(string.Format("https://api.twitch.tv/helix/users/follows?from_id={0}", id)));
            if (response != null)
                LoadChannelFollowedByResponse(response, ref ret);
            return ret;
        }

        public static List<string> GetChannelFollowedByLogin(string login)
        {
            UserInfo? broadcasterInfo = GetUserInfoFromLogin(login);
            if (broadcasterInfo != null)
                return GetChannelFollowedByID(broadcasterInfo.ID);
            return new();
        }
    }
}
