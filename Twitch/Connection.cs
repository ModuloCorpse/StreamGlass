﻿using Quicksand.Web;
using StreamGlass.Profile;
using StreamGlass.StreamChat;
using StreamFeedstock.Controls;
using ChatClient = StreamGlass.Twitch.IRC.ChatClient;
using StreamFeedstock;
using StreamGlass.StreamAlert;
using StreamGlass.Http;
using StreamGlass.Connections;
using StreamGlass.Settings;
using StreamFeedstock.Placeholder;
using System;

namespace StreamGlass.Twitch
{
    public class Connection : IStreamConnection
    {
        private readonly Data m_Settings;
        private readonly ChatClient m_Client;
        private ChannelInfo? m_OriginalBroadcasterChannelInfo = null;
        private readonly Authenticator m_Authenticator;
        private readonly StreamGlassWindow m_Form;
        private readonly EventSub m_EventSub;
        private string m_Channel = "";
        private bool m_IsConnected = false;

        public Connection(Server webServer, Data settings, StreamGlassWindow form)
        {
            m_Settings = settings;
            m_Form = form;
            m_Authenticator = new(webServer, settings);
            //This section will create the settings variables ONLY if they where not loaded
            m_Settings.Create("twitch", "auto_connect", "false");
            m_Settings.Create("twitch", "browser", "");
            m_Settings.Create("twitch", "public_key", "");
            m_Settings.Create("twitch", "secret_key", "");
            m_Settings.Create("twitch", "sub_mode", "all");

            m_Client = new(m_Settings);
            m_EventSub = new(m_Settings);

            if (m_Settings.Get("twitch", "auto_connect") == "true")
                Connect();
        }

        private void Register()
        {
            CanalManager.Register(StreamGlassCanals.CHAT_CONNECTED, OnConnected);
            CanalManager.Register<string>(StreamGlassCanals.CHAT_JOINED, OnJoinedChannel);
            CanalManager.Register<string>(StreamGlassCanals.USER_JOINED, OnUserJoinedChannel);
            CanalManager.Register<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);

            ChatCommand.AddFunction("Game", (string[] variables) => {
                var channelInfo = API.GetChannelInfoFromLogin(variables[0]);
                if (channelInfo != null)
                    return channelInfo.GameName;
                return variables[0];
            });
            ChatCommand.AddFunction("DisplayName", (string[] variables) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.DisplayName;
                return variables[0];
            });
            ChatCommand.AddFunction("Channel", (string[] variables) => {
                var userInfo = API.GetUserInfoFromLogin(variables[0]);
                if (userInfo != null)
                    return userInfo.Login;
                return variables[0];
            });
        }

        private void Unregister()
        {
            CanalManager.Unregister(StreamGlassCanals.CHAT_CONNECTED, OnConnected);
            CanalManager.Unregister<string>(StreamGlassCanals.CHAT_JOINED, OnJoinedChannel);
            CanalManager.Unregister<string>(StreamGlassCanals.USER_JOINED, OnUserJoinedChannel);
            CanalManager.Unregister<UpdateStreamInfoArgs>(StreamGlassCanals.UPDATE_STREAM_INFO, SetStreamInfo);
            ChatCommand.RemoveFunction("Game");
            ChatCommand.RemoveFunction("DisplayName");
            ChatCommand.RemoveFunction("Channel");
        }

        public void Connect()
        {
            if (!m_IsConnected)
            {
                Register();
                OAuthToken? apiToken = m_Authenticator.Authenticate();
                if (apiToken != null)
                {
                    m_IsConnected = true;
                    m_EventSub.SetToken(apiToken);
                    API.Authenticate(apiToken);
                    API.LoadGlobalEmoteSet();
                    API.LoadChannelEmoteSetFromLogin("chaporon_");
                    OAuthToken? ircToken;
                    string browser = m_Settings.Get("twitch", "browser");
                    if (string.IsNullOrWhiteSpace(browser))
                        ircToken = apiToken;
                    else
                        ircToken = m_Authenticator.Authenticate(browser);
                    if (ircToken != null)
                    {
                        UserInfo? userInfoOfToken = API.GetUserInfoOfToken(ircToken);
                        m_Client.SetSelfUserInfo(userInfoOfToken);
                        if (userInfoOfToken != null)
                            API.LoadEmoteSetFromFollowedChannelOfID(userInfoOfToken.ID);
                        m_Client.Init("StreamGlass", ircToken);
                    }
                }
            }
            else
            {
                m_Client.Reconnect();
            }
        }

        internal void ReconnectEventSub() => m_EventSub.Reconnect();

        public void Disconnect()
        {
            m_EventSub.Disconnect();
            m_Client.Disconnect();
            ResetStreamInfo();
            Unregister();
        }

        public TabItemContent[] GetSettings() => new TabItemContent[] { new TwitchSettingsItem(m_Settings, this) };

        private void OnConnected(int _)
        {
            UserInfo? selfUserInfo = API.GetSelfUserInfo();
            if (selfUserInfo != null)
                m_Client.Join(selfUserInfo.Login);
        }

        private void OnJoinedChannel(int _, object? arg)
        {
            if (arg == null)
                return;
            string channel = (string)arg!;
            if (m_Channel != channel)
            {
                m_Channel = channel;
                API.LoadChannelEmoteSetFromLogin(channel[1..]);
                ResetStreamInfo();
                m_OriginalBroadcasterChannelInfo = API.GetChannelInfoFromLogin(channel[1..]);
                m_Form.JoinChannel(channel);
                if (m_OriginalBroadcasterChannelInfo != null)
                    m_EventSub.Connect(m_OriginalBroadcasterChannelInfo.Broadcaster.ID);
            }
        }

        public void Update(long _) {}

        private void OnUserJoinedChannel(int _, object? arg)
        {
            if (arg == null)
                return;
            string login = (string)arg!;
            API.LoadEmoteSetFromFollowedChannelOfLogin(login);
        }

        public void SendMessage(string channel, string message) => m_Client.SendMessage(channel, message);

        public string GetEmoteURL(string emoteID, BrushPaletteManager palette)
        {
            return API.GetEmoteURL(emoteID, palette.GetPaletteType());
        }

        private void ResetStreamInfo()
        {
            if (m_OriginalBroadcasterChannelInfo != null)
                API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
        }

        private void SetStreamInfo(int _, object? arg)
        {
            if (arg == null)
                return;
            UpdateStreamInfoArgs args = (UpdateStreamInfoArgs)arg!;
            if (m_OriginalBroadcasterChannelInfo != null)
            {
                if (string.IsNullOrWhiteSpace(args.Title) && string.IsNullOrWhiteSpace(args.Category.ID) && string.IsNullOrWhiteSpace(args.Language))
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, m_OriginalBroadcasterChannelInfo.Title, m_OriginalBroadcasterChannelInfo.GameID, m_OriginalBroadcasterChannelInfo.BroadcasterLanguage);
                else
                    API.SetChannelInfoFromID(m_OriginalBroadcasterChannelInfo.Broadcaster.ID, args.Title, args.Category.ID, args.Language);
            }
        }

        public Profile.CategoryInfo? SearchCategoryInfo(Window parent, Profile.CategoryInfo? info)
        {
            CategorySearchDialog dialog = new(parent, info);
            dialog.ShowDialog();
            return dialog.CategoryInfo;
        }

        public void Test()
        {
        }
    }
}
