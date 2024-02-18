﻿using CorpseLib.Placeholder;
using CorpseLib.StructuredText;
using StreamGlass.Core;
using StreamGlass.Core.Audio;
using StreamGlass.Core.Controls;
using StreamGlass.Twitch.Events;
using TwitchCorpse;

namespace StreamGlass.Twitch.Alert
{
    public class AlertScrollPanel : ScrollPanel<AlertControl>
    {
        public enum AlertType
        {
            INCOMMING_RAID,
            OUTGOING_RAID,
            DONATION,
            REWARD,
            FOLLOW,
            TIER1,
            TIER2,
            TIER3,
            TIER4,
            SHOUTOUT,
            BEING_SHOUTOUT
        }

        internal class AlertInfo
        {
            private readonly Sound? m_Audio;
            private readonly string m_ImgPath;
            private readonly string m_Prefix;
            private readonly string m_ChatMessage;
            private readonly bool m_IsEnabled;
            private readonly bool m_HaveChatMessage;

            public Sound? Audio => m_Audio;
            public string ImgPath => m_ImgPath;
            public string Prefix => m_Prefix;
            public string ChatMessage => m_ChatMessage;
            public bool IsEnabled => m_IsEnabled;
            public bool HaveChatMessage => m_HaveChatMessage;

            public AlertInfo(Sound? audio, string imgPath, string prefix, bool isEnabled)
            {
                m_Audio = audio;
                m_ImgPath = imgPath;
                m_Prefix = prefix;
                m_ChatMessage = string.Empty;
                m_IsEnabled = isEnabled;
                m_HaveChatMessage = false;
            }

            public AlertInfo(Sound? audio, string imgPath, string prefix, string chatMessage, bool isEnabled, bool haveChatMessage)
            {
                m_Audio = audio;
                m_ImgPath = imgPath;
                m_Prefix = prefix;
                m_ChatMessage = chatMessage;
                m_IsEnabled = isEnabled;
                m_HaveChatMessage = haveChatMessage;
            }
        }

        private readonly AlertInfo[] m_GiftAlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private readonly AlertInfo[] m_AlertInfo = new AlertInfo[Enum.GetNames(typeof(AlertType)).Length];
        private double m_MessageContentFontSize = 20;

        public AlertScrollPanel() : base() { }

        public void Init()
        {
            StreamGlassCanals.Register<DonationEventArgs>(TwitchPlugin.Canals.DONATION, OnDonation);
            StreamGlassCanals.Register<FollowEventArgs>(TwitchPlugin.Canals.FOLLOW, OnNewFollow);
            StreamGlassCanals.Register<GiftFollowEventArgs>(TwitchPlugin.Canals.GIFT_FOLLOW, OnNewGiftFollow);
            StreamGlassCanals.Register<RaidEventArgs>(TwitchPlugin.Canals.RAID, OnRaid);
            StreamGlassCanals.Register<RewardEventArgs>(TwitchPlugin.Canals.REWARD, OnReward);
            StreamGlassCanals.Register<ShoutoutEventArgs>(TwitchPlugin.Canals.SHOUTOUT, OnShoutout);
            StreamGlassCanals.Register<TwitchUser>(TwitchPlugin.Canals.BEING_SHOUTOUT, OnBeingShoutout);
        }

        internal double MessageContentFontSize => m_MessageContentFontSize;

        public void SetContentFontSize(double fontSize)
        {
            m_MessageContentFontSize = fontSize;
            foreach (AlertControl message in Controls)
                message.SetMessageFontSize(m_MessageContentFontSize);
            UpdateControlsPosition();
        }

        public void SetGiftAlertInfo(AlertType alertType, Sound? audio, string imgPath, string prefix, string chatMessage, bool isEnabled, bool haveChatMessage) => m_GiftAlertInfo[(int)alertType] = new(audio, imgPath, prefix, chatMessage, isEnabled, haveChatMessage);
        internal void SetGiftAlertInfo(AlertType alertType, AlertInfo alertInfo) => m_GiftAlertInfo[(int)alertType] = new(alertInfo.Audio, alertInfo.ImgPath, alertInfo.Prefix, alertInfo.IsEnabled);
        internal AlertInfo GetGiftAlertInfo(AlertType alertType) => m_GiftAlertInfo[(int)alertType];

        public void SetAlertInfo(AlertType alertType, Sound? audio, string imgPath, string prefix, string chatMessage, bool isEnabled, bool haveChatMessage) => m_AlertInfo[(int)alertType] = new(audio, imgPath, prefix, chatMessage, isEnabled, haveChatMessage);
        internal void SetAlertInfo(AlertType alertType, AlertInfo alertInfo) => m_AlertInfo[(int)alertType] = new(alertInfo.Audio, alertInfo.ImgPath, alertInfo.Prefix, alertInfo.IsEnabled);
        internal AlertInfo GetAlertInfo(AlertType alertType) => m_AlertInfo[(int)alertType];


        private void NewAlert(AlertInfo alertInfo, object e, Text? message = null)
        {
            Dispatcher.Invoke(() =>
            {
                StreamGlassContext.LOGGER.Log("Adding visual alert");
                if (alertInfo.IsEnabled)
                {
                    StreamGlassContext context = new();
                    context.AddVariable("e", e);
                    string alertPrefix = Converter.Convert(alertInfo.Prefix, context);
                    Text alertMessage = new(alertPrefix);
                    if (message != null && !message.IsEmpty)
                    {
                        alertMessage.AddText(": ");
                        alertMessage.Append(message);
                    }
                    Alert alert = new(alertInfo.ImgPath, alertMessage);
                    AlertControl alertControl = new(alert, m_MessageContentFontSize);
                    alertControl.AlertMessage.Loaded += (sender, e) => { UpdateControlsPosition(); };
                    AddControl(alertControl);

                    if (alertInfo.HaveChatMessage)
                        StreamGlassCanals.Emit(StreamGlassCanals.SEND_MESSAGE, Converter.Convert(alertInfo.ChatMessage, context));

                    if (alertInfo.Audio != null)
                        SoundManager.PlaySound(alertInfo.Audio);
                }
            });
        }

        private void OnNewFollow(FollowEventArgs? obj)
        {
            FollowEventArgs e = obj!;
            StreamGlassContext.LOGGER.Log("New follow alert");
            switch (e.Tier)
            {
                case 0: NewAlert(m_AlertInfo[(int)AlertType.FOLLOW], e, e.Message); break;
                case 1: NewAlert(m_AlertInfo[(int)AlertType.TIER1], e, e.Message); break;
                case 2: NewAlert(m_AlertInfo[(int)AlertType.TIER2], e, e.Message); break;
                case 3: NewAlert(m_AlertInfo[(int)AlertType.TIER3], e, e.Message); break;
                case 4: NewAlert(m_AlertInfo[(int)AlertType.TIER4], e, e.Message); break;
            }
        }

        private void OnNewGiftFollow(GiftFollowEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New gift follow alert");
            GiftFollowEventArgs e = obj!;
            switch (e.Tier)
            {
                case 0: NewAlert(m_GiftAlertInfo[(int)AlertType.FOLLOW], e, e.Message); break;
                case 1: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER1], e, e.Message); break;
                case 2: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER2], e, e.Message); break;
                case 3: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER3], e, e.Message); break;
                case 4: NewAlert(m_GiftAlertInfo[(int)AlertType.TIER4], e, e.Message); break;
            }
        }

        private void OnDonation(DonationEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New donation alert");
            NewAlert(m_AlertInfo[(int)AlertType.DONATION], obj!, (obj!).Message);
        }

        private void OnRaid(RaidEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New raid alert");
            NewAlert(m_AlertInfo[(int)((obj!).IsIncomming ? AlertType.INCOMMING_RAID : AlertType.OUTGOING_RAID)], obj!);
        }

        private void OnReward(RewardEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New reward alert");
            NewAlert(m_AlertInfo[(int)AlertType.REWARD], obj!);
        }

        private void OnShoutout(ShoutoutEventArgs? obj)
        {
            StreamGlassContext.LOGGER.Log("New shoutout alert");
            NewAlert(m_AlertInfo[(int)AlertType.SHOUTOUT], obj!);
        }

        private void OnBeingShoutout(TwitchUser? obj)
        {
            StreamGlassContext.LOGGER.Log("New received shoutout alert");
            NewAlert(m_AlertInfo[(int)AlertType.BEING_SHOUTOUT], obj!);
        }
    }
}