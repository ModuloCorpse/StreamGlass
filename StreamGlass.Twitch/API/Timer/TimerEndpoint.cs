using CorpseLib;
using CorpseLib.Json;
using CorpseLib.Web.API;
using CorpseLib.Web.Http;
using StreamGlass.Core;
using System.IO;

namespace StreamGlass.Twitch.API.Timer
{
    public class TimerEndpoint : AHTTPEndpoint
    {
        private class FileCountdownTimeAction : TimedAction
        {
            private readonly string m_FilePath;
            private readonly string m_FinishMessage;

            public FileCountdownTimeAction(string filePath, long durationInSeconds) : base(250, (durationInSeconds - 1) * 1000)
            {
                m_FilePath = filePath;
                m_FinishMessage = string.Empty;
            }

            public FileCountdownTimeAction(string filePath, string finishMessage, long durationInSeconds) : base(250, (durationInSeconds - 1) * 1000)
            {
                m_FilePath = filePath;
                m_FinishMessage = finishMessage;
            }

            protected override void OnActionStart()
            {
                base.OnActionStart();
                OnActionUpdate(0);
            }

            protected override void OnActionUpdate(long elapsed)
            {
                TimeSpan remainingTime = TimeSpan.FromMilliseconds((Duration + 1000) - elapsed);
                string remainingStr = string.Format("{0:D2}:{1:D2}", remainingTime.Minutes, remainingTime.Seconds);
                if (!string.IsNullOrEmpty(m_FilePath))
                    File.WriteAllText(m_FilePath, remainingStr);
                base.OnActionUpdate(elapsed);
            }

            protected override void OnActionFinish()
            {
                if (!string.IsNullOrEmpty(m_FilePath) && !string.IsNullOrEmpty(m_FinishMessage))
                    File.WriteAllText(m_FilePath, m_FinishMessage);
                base.OnActionFinish();
            }
        }

        private readonly Dictionary<string, FileCountdownTimeAction> m_Timers = [];
        private readonly HashSet<string> m_FileToClear = [];

        public TimerEndpoint() : base("/timer") { }

        ~TimerEndpoint()
        {
            foreach (var pair in m_Timers)
                pair.Value.Stop();
            foreach (string path in m_FileToClear)
                File.WriteAllText(path, string.Empty);
        }

        protected override Response OnPostRequest(Request request)
        {
            try
            {
                JFile jfile = new(request.Body);
                string path = jfile.Get<string>("path")!;
                if (jfile.TryGet("duration", out long duration))
                {
                    string endMessage = jfile.GetOrDefault("end", string.Empty);
                    if (m_Timers.TryGetValue(path, out var oldTimer))
                        oldTimer.Stop();
                    FileCountdownTimeAction timer = string.IsNullOrEmpty(endMessage) ? new(path, duration) : new(path, endMessage, duration);
                    timer.OnFinish += (sender, e) => m_Timers.Remove(path);
                    timer.OnStop += (sender, e) => m_Timers.Remove(path);
                    m_Timers[path] = timer;
                    m_FileToClear.Add(path);
                    timer.Start();
                    if (jfile.TryGet("ads_duration", out uint adsDuration))
                    {
                        if (jfile.TryGet("ads_delay", out int adsDelay))
                            Task.Delay(adsDelay * 1000).ContinueWith(t => StreamGlassCanals.Emit("start_ads", adsDuration));
                        else
                            StreamGlassCanals.Emit("start_ads", adsDuration);
                    }
                }
                else
                {
                    if (m_Timers.TryGetValue(path, out FileCountdownTimeAction? timeAction))
                        timeAction.Stop();
                    File.WriteAllText(path, string.Empty);
                }
            }
            catch
            {
                return new(400, "Bad Request", "Request body isn't a well-formed json");
            }
            return base.OnPostRequest(request);
        }
    }
}
