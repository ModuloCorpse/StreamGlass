using StreamFeedstock;
using StreamFeedstock.Controls;
using StreamGlass.Http;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamGlass.Twitch
{
    public class API
    {
        private readonly HashSet<string> ms_LoadedChannelIDEmoteSets = new();
        private readonly HashSet<string> ms_LoadedChannelEmoteSets = new();
        private readonly HashSet<string> ms_LoadedEmoteSets = new();
        private readonly APICache ms_Cache = new();
        private OAuthToken? ms_AccessToken = null;
        private User ms_SelfUserInfo = new("", "", User.Type.BROADCASTER);
        private string ms_EmoteURLTemplate = "";

        internal void Authenticate(OAuthToken accessToken)
        {
            ms_AccessToken = accessToken;
            GetRequest request = new("https://api.twitch.tv/helix/users", accessToken);
            request.Send();
            User? user = GetUserInfo(request.GetResponse(), User.Type.BROADCASTER);
            if (user != null)
            {
                ms_SelfUserInfo = user;
                LoadChannelEmoteSet(user);
            }
        }

        private Json LoadEmoteSetContent(string content)
        {
            Json responseJson = new(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            foreach (Json data in datas)
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
            return responseJson;
        }

        public void LoadGlobalEmoteSet()
        {
            GetRequest request = new("https://api.twitch.tv/helix/chat/emotes/global", ms_AccessToken);
            request.Send();
            Json responseJson = LoadEmoteSetContent(request.GetResponse());
            if (responseJson.TryGet("template", out string? template))
                ms_EmoteURLTemplate = template!;
        }

        public void LoadChannelEmoteSet(User user)
        {
            if (ms_LoadedChannelIDEmoteSets.Contains(user.ID))
                return;
            ms_LoadedChannelIDEmoteSets.Add(user.ID);
            GetRequest request = new(string.Format("https://api.twitch.tv/helix/chat/emotes?broadcaster_id={0}", user.ID), ms_AccessToken);
            request.Send();
            LoadEmoteSetContent(request.GetResponse());
        }

        public void LoadEmoteSet(string emoteSetID)
        {
            if (ms_LoadedEmoteSets.Contains(emoteSetID))
                return;
            ms_LoadedEmoteSets.Add(emoteSetID);
            GetRequest request = new(string.Format("https://api.twitch.tv/helix/chat/emotes/set?emote_set_id={0}", emoteSetID), ms_AccessToken);
            request.Send();
            LoadEmoteSetContent(request.GetResponse());
        }

        public void LoadEmoteSetFromFollowedChannel(User user)
        {
            List<User> followedBy = GetChannelFollowedByID(user);
            foreach (User followed in followedBy)
                LoadChannelEmoteSet(followed);
        }

        public string GetEmoteURL(string id, BrushPalette.Type paletteType)
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

        public EmoteInfo? GetEmoteFromID(string id)
        {
            EmoteInfo? emoteInfo = ms_Cache.GetEmote(id);
            if (emoteInfo != null)
                return emoteInfo;
            return null;
        }

        public User.Type GetUserType(bool self, bool mod, string type, string id)
        {
            User.Type userType = User.Type.NONE;
            if (self)
                userType = User.Type.SELF;
            else
            {
                if (mod)
                    userType = User.Type.MOD;
                switch (type)
                {
                    case "admin": userType = User.Type.ADMIN; break;
                    case "global_mod": userType = User.Type.GLOBAL_MOD; break;
                    case "staff": userType = User.Type.STAFF; break;
                }
                if (id == (ms_SelfUserInfo?.ID ?? ""))
                    userType = User.Type.BROADCASTER;
            }
            return userType;
        }

        public User.Type GetUserType(bool self, string type, string id)
        {
            bool isMod = false;
            if (ms_SelfUserInfo != null)
            {
                GetRequest request = new(string.Format("https://api.twitch.tv/helix/moderation/moderators?broadcaster_id={0}&user_id={1}", ms_SelfUserInfo.ID, id), ms_AccessToken);
                request.Send();
                Json json = new(request.GetResponse());
                isMod = json.GetList<Json>("data").Count != 0;
            }
            return GetUserType(self, isMod, type, id);
        }

        private User? GetUserInfo(string content, User.Type? userType)
        {
            Json responseJson = new(content);
            List<Json> datas = responseJson.GetList<Json>("data");
            if (datas.Count > 0)
            {
                Json data = datas[0];
                if (data.TryGet("id", out string? id) &&
                    data.TryGet("login", out string? login) &&
                    data.TryGet("display_name", out string? displayName) &&
                    data.TryGet("type", out string? type))
                    return new(id!, login!, displayName!, (userType != null) ? (User.Type)userType! : GetUserType(false, type!, id!));
            }
            return null;
        }

        public User? GetUserInfoOfToken(OAuthToken token)
        {
            GetRequest request = new("https://api.twitch.tv/helix/users", token);
            request.Send();
            return GetUserInfo(request.GetResponse(), User.Type.SELF);
        }

        public User GetSelfUserInfo() => ms_SelfUserInfo;

        public User? GetUserInfoFromLogin(string login)
        {
            User? userInfo = null;
            userInfo ??= ms_Cache.GetUserInfoByLogin(login);
            if (userInfo != null)
                return userInfo;
            GetRequest request = new(string.Format("https://api.twitch.tv/helix/users?login={0}", login), ms_AccessToken);
            request.Send();
            User? ret = GetUserInfo(request.GetResponse(), null);
            if (ret != null)
                ms_Cache.AddUserInfo(ret);
            return ret;
        }

        public ChannelInfo? GetChannelInfo(string login)
        {
            ChannelInfo? channelInfo = null;
            if (channelInfo != null)
                return channelInfo;
            User? broadcasterInfo = GetUserInfoFromLogin(login);
            if (broadcasterInfo != null)
            {
                GetRequest request = new(string.Format("https://api.twitch.tv/helix/channels?broadcaster_id={0}", broadcasterInfo.ID), ms_AccessToken);
                request.Send();

                Json responseJson = new(request.GetResponse());
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
            }
            return null;
        }

        public bool SetChannelInfo(User user, string title, string gameID, string language = "")
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
            PatchRequest request = new(string.Format("https://api.twitch.tv/helix/channels?broadcaster_id={0}", user.ID), body.ToString(), "application/json", ms_AccessToken);
            request.Send();
            return (request.GetStatusCode() == 204);
        }

        public List<User> GetChannelFollowedByID(User user)
        {
            List<User> ret = new();
            GetRequest request = new(string.Format("https://api.twitch.tv/helix/users/follows?from_id={0}", user.ID), ms_AccessToken);
            request.Send();
            Json responseJson = new(request.GetResponse());
            foreach (Json data in responseJson.GetList<Json>("data"))
            {
                if (data.TryGet("to_id", out string? toID) &&
                    data.TryGet("to_login", out string? toLogin) &&
                    data.TryGet("to_name", out string? toName))
                    ret.Add(new(toID!, toLogin!, toName!, GetUserType(false, "", toID!)));
            }
            return ret;
        }

        public List<CategoryInfo> SearchCategoryInfo(string query)
        {
            List<CategoryInfo> ret = new();
            GetRequest request = new(string.Format("https://api.twitch.tv/helix/search/categories?query={0}", HttpUtility.UrlEncode(query)), ms_AccessToken);
            request.Send();
            Json responseJson = new(request.GetResponse());
            foreach (Json data in responseJson.GetList<Json>("data"))
            {
                if (data.TryGet("id", out string? id) &&
                    data.TryGet("name", out string? name) &&
                    data.TryGet("box_art_url", out string? imageURL))
                    ret.Add(new(id!, name!, imageURL!));
            }
            return ret;
        }

        public bool ManageHeldMessage(string senderID, string messageID, bool allow)
        {
            Json json = new();
            json.Set("user_id", senderID);
            json.Set("msg_id", messageID);
            json.Set("action", (allow) ? "ALLOW" : "DENY");
            PostRequest request = new("https://api.twitch.tv/helix/moderation/automod/message", json, ms_AccessToken);
            request.Send();
            return (request.GetStatusCode() == 204);
        }

        public bool BanUser(User user, string reason, uint duration = 0)
        {
            if (ms_SelfUserInfo == null)
                return false;
            Json json = new();
            Json data = new();
            data.Set("user_id", user.ID);
            if (duration > 0)
                data.Set("duration", duration);
            data.Set("reason", reason);
            json.Set("data", data);
            PostRequest request = new(string.Format("https://api.twitch.tv/helix/moderation/bans?broadcaster_id={0}&moderator_id={0}", ms_SelfUserInfo.ID), json, ms_AccessToken);
            request.Send();
            return (request.GetStatusCode() == 200);
        }

        public bool UnbanUser(string moderatorID, string userID)
        {
            if (ms_SelfUserInfo == null)
                return false;
            DeleteRequest request = new(string.Format("https://api.twitch.tv/helix/moderation/bans?broadcaster_id={0}&moderator_id={1}&user_id={2}", ms_SelfUserInfo.ID, moderatorID, userID), ms_AccessToken);
            request.Send();
            return (request.GetStatusCode() == 204);
        }
    }
}
